using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping
{
    public class TraceLayoutEngine
    {
        private const double OffsetStep = -1.5f;

        public IEnumerable<AssignedTrace> Layout(IEnumerable<TraceDescriptor> descriptors)
        {
            var result = new List<AssignedTrace>();
            double offset = -3.0f;

            // group by entity: Gateway, IU:1, IU:2...
            var groups = descriptors
                .GroupBy(d => d.EntityId ?? "Global")
                .OrderBy(g => GetGroupPriority(g.Key))
                .ThenBy(g => g.Key);

            foreach (var group in groups)
            {
                // inside each group: Temperature, State, Setpoint...
                foreach (var desc in group.OrderBy(d => d.Category))
                {
                    if (desc.DrawStyle == DrawStyles.Lines)
                    {
                        result.Add(new AssignedTrace
                        {
                            Descriptor = desc,
                            VerticalOffset = 0,
                        });
                    }

                    if (desc.DrawStyle == DrawStyles.State)
                    {
                        result.Add(new AssignedTrace
                        {
                            Descriptor = desc,
                            VerticalOffset = offset,
                        });
                        offset += OffsetStep;
                    }
                }
            }

            return result;
        }





        private int GetGroupPriority(string groupId)
        {
            if (groupId.StartsWith("HVAC:"))
                return 3;

            if (groupId.StartsWith("THR:"))
                return 2;

            if (groupId.StartsWith("HEATMAN:"))
                return 1;

            if (groupId.StartsWith("SHM:"))
                return 0;

            return -1;
        }
    }
}


