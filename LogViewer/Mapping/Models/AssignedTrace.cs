using FormsLib.Scope;

namespace LogViewer.Mapping.Models
{
    public class AssignedTrace
    {
        public AssignedTrace(TraceDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        public TraceDescriptor Descriptor { get; }
        public Trace Trace { get; set; } = null!;
        public ScopeController ScopeController { get; set; } = null!;

        public override string ToString() => Descriptor.TraceId;
    }
}