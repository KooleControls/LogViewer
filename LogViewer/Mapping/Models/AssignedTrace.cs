namespace LogViewer.Mapping.Models
{
    public class AssignedTrace
    {
        public TraceDescriptor Descriptor { get; set; } = null!;
        public double VerticalOffset { get; set; }
        public double Scale { get; set; } = 10.0;
        public string? Unit { get; set; }

        public override string ToString()
        {
            return Descriptor.TraceId;
        }
    }
}
