using LogViewer.Mapping.Interfaces;
using LogViewer.Mapping.Models;

namespace LogViewer.Mapping
{
    /// <summary>
    /// Qualifies trace identities with the device's parent, so the same component behind two
    /// different parents lands in two traces instead of one interleaved mess.
    ///
    /// The mappers only know their own device ("THR0_TempActual") because that is all a log
    /// entry tells them; this wraps them so they stay parent-agnostic.
    ///
    /// The parent is appended, not prepended, to keep the "THR:"/"HVAC:" prefixes that
    /// <see cref="TraceRegistry"/> groups traces by intact.
    /// </summary>
    public sealed class DeviceQualifiedTraceBuilder : ITraceBuilder
    {
        private readonly ITraceBuilder _inner;
        private readonly string _parent;

        public DeviceQualifiedTraceBuilder(ITraceBuilder inner, string parent)
        {
            _inner = inner;
            _parent = parent;
        }

        public IReadOnlyList<AssignedTrace> Traces => _inner.Traces;

        public AssignedTrace GetOrCreate(TraceDescriptor descriptor)
        {
            descriptor.TraceId = $"{descriptor.TraceId}@{_parent}";

            if (!string.IsNullOrEmpty(descriptor.EntityId))
                descriptor.EntityId = $"{descriptor.EntityId}@{_parent}";

            return _inner.GetOrCreate(descriptor);
        }

        public void Clear() => _inner.Clear();
    }
}
