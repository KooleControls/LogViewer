using LogViewer.Devices.Gateway;
using LogViewer.Logging;

namespace LogViewer.Mapping
{
    [Flags]
    public enum CommunicationProblems
    {
        None = 0,
        NoSntp = 1 << 0,            // gateway offline or broken
        NoThermostat = 1 << 1,      // OpenTherm thermostat silent
        NoIndoorUnit = 1 << 2,      // Modbus indoor unit silent
        OpenThermTimeout = 1 << 3,
        ModbusError = 1 << 4,
        IndoorUnitFault = 1 << 5,
    }

    /// <summary>From <see cref="Time"/> on, until the next change, the house had these problems.</summary>
    public record ProblemChange(DateTime Time, CommunicationProblems Problems);

    /// <summary>
    /// Turns one house's log into a timeline of communication problems: periods in which a part
    /// stayed silent for longer than the allowed gap, and periods in which the gateway itself
    /// reported an error state (OpenTherm timeout, Modbus error, indoor unit fault).
    /// </summary>
    public static class CommunicationGapAnalyzer
    {
        public static List<ProblemChange> Analyze(IReadOnlyList<LogEntry> entries, DateTime from, DateTime until, TimeSpan maxGap)
        {
            var sorted = entries.OrderBy(e => e.TimeStamp).ToList();
            var periods = new List<(DateTime Start, DateTime End, CommunicationProblems Problem)>();

            periods.AddRange(SilentPeriods(sorted, from, until, maxGap, CommunicationProblems.NoSntp,
                GatewayLogCodes.x3E_TCPIP_SNTP_INFO));
            periods.AddRange(SilentPeriods(sorted, from, until, maxGap, CommunicationProblems.NoThermostat,
                GatewayLogCodes.Therm_TempActualChanged));
            periods.AddRange(SilentPeriods(sorted, from, until, maxGap, CommunicationProblems.NoIndoorUnit,
                GatewayLogCodes.CMN_IndoorUnit_ActualTempChanged, GatewayLogCodes.Hvac_ActualTempChanged));

            // These codes log a value when the state changes; non-zero means the error is active.
            periods.AddRange(ReportedPeriods(sorted, until, CommunicationProblems.OpenThermTimeout,
                GatewayLogCodes.Therm_CommunicationTimeout));
            periods.AddRange(ReportedPeriods(sorted, until, CommunicationProblems.ModbusError,
                GatewayLogCodes.CMN_ModbusError));
            periods.AddRange(ReportedPeriods(sorted, until, CommunicationProblems.IndoorUnitFault,
                GatewayLogCodes.Hvac_IsFaultChanged));

            return ToTimeline(periods, from);
        }

        public static string ToText(CommunicationProblems problems)
        {
            if (problems == CommunicationProblems.None)
                return "OK";

            var parts = new List<string>();
            if (problems.HasFlag(CommunicationProblems.NoSntp)) parts.Add("NO SNTP");
            if (problems.HasFlag(CommunicationProblems.NoThermostat)) parts.Add("NO THERM");
            if (problems.HasFlag(CommunicationProblems.NoIndoorUnit)) parts.Add("NO UNIT");
            if (problems.HasFlag(CommunicationProblems.OpenThermTimeout)) parts.Add("OT TIMEOUT");
            if (problems.HasFlag(CommunicationProblems.ModbusError)) parts.Add("MODBUS ERR");
            if (problems.HasFlag(CommunicationProblems.IndoorUnitFault)) parts.Add("UNIT FAULT");
            return string.Join(" + ", parts);
        }

        /// <summary>
        /// Every stretch longer than <paramref name="maxGap"/> without one of the codes, including
        /// the stretch from the start of the period to the first entry and from the last one to the end.
        /// </summary>
        private static IEnumerable<(DateTime, DateTime, CommunicationProblems)> SilentPeriods(
            List<LogEntry> sorted, DateTime from, DateTime until, TimeSpan maxGap,
            CommunicationProblems problem, params GatewayLogCodes[] codes)
        {
            DateTime lastSeen = from;
            foreach (var entry in sorted.Where(e => codes.Contains((GatewayLogCodes)e.LogCode)))
            {
                if (entry.TimeStamp - lastSeen > maxGap)
                    yield return (lastSeen, entry.TimeStamp, problem);
                lastSeen = entry.TimeStamp;
            }

            if (until - lastSeen > maxGap)
                yield return (lastSeen, until, problem);
        }

        private static IEnumerable<(DateTime, DateTime, CommunicationProblems)> ReportedPeriods(
            List<LogEntry> sorted, DateTime until, CommunicationProblems problem, GatewayLogCodes code)
        {
            DateTime? activeSince = null;
            foreach (var entry in sorted.Where(e => (GatewayLogCodes)e.LogCode == code))
            {
                bool active = (entry.Measurement ?? 0) != 0;
                if (active && activeSince == null)
                    activeSince = entry.TimeStamp;
                else if (!active && activeSince != null)
                {
                    yield return (activeSince.Value, entry.TimeStamp, problem);
                    activeSince = null;
                }
            }

            if (activeSince != null)
                yield return (activeSince.Value, until, problem);
        }

        /// <summary>Merges the (possibly overlapping) periods into one list of moments the combined state changes.</summary>
        private static List<ProblemChange> ToTimeline(
            List<(DateTime Start, DateTime End, CommunicationProblems Problem)> periods, DateTime from)
        {
            var edges = periods
                .SelectMany(p => new[] { (Time: p.Start, p.Problem, Delta: +1), (Time: p.End, p.Problem, Delta: -1) })
                .OrderBy(e => e.Time)
                .ToList();

            // A counter per problem: periods of the same kind never overlap today, but a count stays correct if they do.
            var activeCount = new Dictionary<CommunicationProblems, int>();
            var timeline = new List<ProblemChange> { new(from, CommunicationProblems.None) };

            foreach (var group in edges.GroupBy(e => e.Time))
            {
                foreach (var edge in group)
                    activeCount[edge.Problem] = activeCount.GetValueOrDefault(edge.Problem) + edge.Delta;

                var problems = activeCount
                    .Where(kv => kv.Value > 0)
                    .Aggregate(CommunicationProblems.None, (all, kv) => all | kv.Key);

                if (timeline[^1].Time == group.Key)
                    timeline[^1] = new ProblemChange(group.Key, problems);
                else if (timeline[^1].Problems != problems)
                    timeline.Add(new ProblemChange(group.Key, problems));
            }
            return timeline;
        }
    }
}
