using FormsLib.Scope;
using LogViewer.Logging;
using LogViewer.Mapping;
using LogViewer.Mapping.Models;

public class ScopeControllerAdapter
{
    private readonly ScopeController _scope;
    private readonly TraceFactory _factory = new TraceFactory();

    public ScopeControllerAdapter(ScopeController scope)
    {
        _scope = scope;
    }

    public List<AssignedTrace> LoadAndReturnTraces(
        IEnumerable<AssignedTrace> assignedTraces,
        IEnumerable<LogEntry> entries)
    {
        _scope.Traces.Clear();
        _scope.Labels.Clear();

        var result = new List<AssignedTrace>();

        foreach (var assigned in assignedTraces)
        {
            var built = _factory.CreateTrace(assigned, entries);

            assigned.Built = built;

            _scope.Traces.Add(built.Trace);
            foreach (var label in built.Labels)
                _scope.Labels.Add(label);

            result.Add(assigned);
        }

        return result;
    }
}
