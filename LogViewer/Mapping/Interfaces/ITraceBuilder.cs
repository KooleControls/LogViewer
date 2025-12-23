using LogViewer.Mapping.Models;

namespace LogViewer.Mapping.Interfaces
{
    public interface ITraceBuilder
    {
        AssignedTrace GetOrCreate(TraceDescriptor descriptor);

        IReadOnlyList<AssignedTrace> Traces { get; }

        void Clear();
    }
}