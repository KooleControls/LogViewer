using LogViewer.Mapping.Models;
using System.Runtime.InteropServices;
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
                    else if (desc.DrawStyle == DrawStyles.State)
                    {
                        result.Add(new AssignedTrace
                        {
                            Descriptor = desc,
                            VerticalOffset = offset,
                        });
                        offset += OffsetStep;
                    }
                    else
                    {
                        result.Add(new AssignedTrace
                        {
                            Descriptor = desc,
                            VerticalOffset = 0,
                        });
                    }
                }
            }

            return result;
        }

        private int GetGroupPriority(string groupId)
        {
            var order = new List<string>
            {
                "SHM:",
                "THR:",
                "HVAC:",
            };

            foreach (var group in order) {
                if (groupId.StartsWith(group))
                    return order.IndexOf(group) + 1;
            }

            return 0;
        }
    }
}


