using FormsLib.Scope;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;
using static DevExpress.Data.Filtering.Helpers.SubExprHelper.ThreadHoppingFiltering;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping
{
    public class TraceRegistry : ITraceBuilder
    {
        private const double InitialOffset = 4.0;
        private const double OffsetStep = -1.5;

        private readonly List<AssignedTrace> _traces = new();
        private readonly TraceFactory _factory;
        public IReadOnlyList<AssignedTrace> Traces => _traces;

        public TraceRegistry(ScopeController scope)
        {
            _factory = new TraceFactory(scope);
        }

        public AssignedTrace GetOrCreate(TraceDescriptor descriptor)
        {
            var existing = _traces.FirstOrDefault(t => t.Descriptor.TraceId == descriptor.TraceId);
            if (existing != null)
                return existing;

            var created = _factory.CreateTrace(descriptor);
            _traces.Add(created);
            RecalculateLayout();
            return created;
        }

        public void Clear()
        {
            _traces.Clear();
        }

        private void RecalculateLayout()
        {
            double offset = InitialOffset;

            foreach (var trace in _traces
                         .OrderBy(t => GetGroupOrderId(t.Descriptor.EntityId ?? string.Empty))
                         .ThenBy(t => t.Descriptor.EntityId)
                         .ThenBy(t => t.Descriptor.Category)
                         .ThenBy(t => t.Descriptor.TraceId))
            {

                var desc = trace.Descriptor;
                if (desc.DrawStyle == DrawStyles.Lines || desc.DrawStyle == DrawStyles.NonInterpolatedLine)
                {
                    trace.Trace.Offset = 0;
                }
                else if (desc.DrawStyle == DrawStyles.State)
                {
                    trace.Trace.Offset = offset;
                    offset += OffsetStep;
                }
                else if (desc.DrawStyle == DrawStyles.Cross)
                {
                    trace.Trace.Offset = offset;
                    offset += OffsetStep;
                }
                else
                {
                    trace.Trace.Offset = 0;
                }                
            }
        }

        /// <summary>
        /// Rough grouping order based on entity prefix.
        /// </summary>
        private static int GetGroupOrderId(string groupId)
        {
            var order = new List<string>
            {
                "GIO:",
                "SHM:",
                "THR:",
                "HVAC:",
            };

            foreach (var group in order)
            {
                if (groupId.StartsWith(group, StringComparison.OrdinalIgnoreCase))
                    return order.IndexOf(group) + 1;
            }

            return 0;
        }
    }
}