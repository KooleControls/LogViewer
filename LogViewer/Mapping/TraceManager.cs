using FormsLib.Scope;
using LogViewer.Logging;
using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Mappers;
using LogViewer.Mapping.Models;
using static FormsLib.Scope.Trace;

namespace LogViewer.Mapping
{
    public class TraceManager
    {
        private readonly ScopeController _scope;
        private readonly IReadOnlyList<ITraceMapper> _mappers;
        private readonly TraceRegistry _registry;

        public TraceManager(
            ScopeController scope,
            IEnumerable<ITraceMapper> mappers)
        {
            _scope = scope;
            _mappers = mappers.ToList();
            _registry = new TraceRegistry(_scope);
        }

        public void Clear()
        {
            _registry.Clear();
            _scope.Traces.Clear();
            _scope.Labels.Clear();
            _scope.RedrawAll();
        }

        public void Append(LogEntry entry)
        {
            bool handled = false;

            foreach (var mapper in _mappers)
            {
                handled |= mapper.Map(entry, _registry);
            }

            if (!handled)
            {
                MapUnhandled(entry, _registry);
            }
        }

        public void MapUnhandled(LogEntry entry, ITraceBuilder builder)
        {
            var descriptor = new TraceDescriptor
            {
                TraceId = "Unknown",
                Category = "Unknown",
                EntityId = "Unknown",
                DrawStyle = DrawStyles.Cross,
                DrawOption = DrawOptions.DrawNames,
                BaseColor = Color.Wheat,
                Source = "UnknownMapper"
            };

            var trace = builder.GetOrCreate(descriptor);
            var label = new LinkedLabel(trace.Trace, entry.TimeStamp.Ticks, 1);
            label.Text = $"{entry.AsGatewayLogCode()?.ToString() ?? entry.LogCode.ToString()}";
            trace.ScopeController.Labels.Add(label);
        }

        public void LoadAll(IEnumerable<LogEntry> entries)
        {
            Clear();

            foreach (var entry in entries)
            {
                Append(entry);
            }
        }
    }
}