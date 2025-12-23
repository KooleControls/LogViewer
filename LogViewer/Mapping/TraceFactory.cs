using FormsLib.Maths;
using FormsLib.Scope;
using LogViewer.Mapping.Models;
using static DevExpress.Data.Filtering.Helpers.SubExprHelper.ThreadHoppingFiltering;

namespace LogViewer.Mapping
{
    public class TraceFactory
    {
        private readonly ScopeController _scope;
        public TraceFactory(ScopeController scope)
        {
            _scope = scope;
        }

        public AssignedTrace CreateTrace(TraceDescriptor descriptor)
        {
            AssignedTrace assigned = new(descriptor);
            assigned.Trace = MakeScopeTrace(descriptor);
            assigned.ScopeController = _scope;
            _scope.Traces.Add(assigned.Trace);
            return assigned;
        }


        private Trace MakeScopeTrace(TraceDescriptor descriptor)
        {
            var trace = new Trace
            {
                Name = descriptor.TraceId,
                Tag = descriptor.EntityId,
                Color = descriptor.BaseColor,
                DrawStyle = descriptor.DrawStyle,
                DrawOption = descriptor.DrawOption,
                ToHumanReadable = descriptor.ToHumanReadable,
                Scale = 10.0f,
                Offset = 0.0f
            };
            return trace;
        }
    }
}
