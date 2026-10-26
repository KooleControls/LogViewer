using FormsLib.Scope;
using LogViewer.Devices.Gateway;
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

        /// <summary>Categories to keep visible, or null for all of them.</summary>
        private HashSet<string>? _categoryFilter;

        /// <summary>Trace count the filter was last applied to, so Append can skip the no-op case.</summary>
        private int _filteredTraceCount = -1;

        /// <summary>
        /// Hides every trace whose <see cref="TraceDescriptor.Category"/> isn't listed. Pass nothing
        /// to show all. Sticky: it survives the next fetch, which would otherwise rebuild every
        /// trace with Visible back on.
        /// </summary>
        public void SetCategoryFilter(params string[] categories)
        {
            _categoryFilter = categories.Length == 0
                ? null
                : new HashSet<string>(categories, StringComparer.OrdinalIgnoreCase);

            ApplyCategoryFilter(force: true);
        }

        private void ApplyCategoryFilter(bool force = false)
        {
            // Traces only appear, never vanish, between Appends - so an unchanged count means
            // nothing new to hide.
            if (!force && _filteredTraceCount == _registry.Traces.Count)
                return;

            foreach (var assigned in _registry.Traces)
            {
                assigned.Trace.Visible = _categoryFilter == null
                    || _categoryFilter.Contains(assigned.Descriptor.Category);
            }

            _filteredTraceCount = _registry.Traces.Count;
        }

        public void Clear()
        {
            _registry.Clear();
            _scope.Traces.Clear();
            _scope.Labels.Clear();
            _filteredTraceCount = -1;
            _scope.RedrawAll();
        }

        public void Append(LogEntry entry)
        {
            bool handled = false;
            var builder = BuilderFor(entry);

            foreach (var mapper in _mappers)
            {
                handled |= mapper.Map(entry, builder);
            }

            if (!handled)
            {
                MapUnhandled(entry, builder);
            }

            ApplyCategoryFilter();
        }

        /// <summary>
        /// Entries whose device has a parent get their traces qualified with it, so two gateways
        /// don't share a trace. Entries without one (older files, sources that don't report it)
        /// keep the unqualified names they always had.
        /// </summary>
        private ITraceBuilder BuilderFor(LogEntry entry)
        {
            var parent = entry.Device.Parent;
            return parent is null
                ? _registry
                : new DeviceQualifiedTraceBuilder(_registry, parent.ToString());
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