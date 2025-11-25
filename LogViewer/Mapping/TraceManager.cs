using FormsLib.Scope;
using LogViewer.Logging;
using LogViewer.Mapping.Models;
using System;

namespace LogViewer.Mapping
{
    public class TraceManager
    {
        private readonly ScopeController _scope;
        private readonly TracePipeline _pipeline;
        private readonly ScopeControllerAdapter _adapter;

        public TraceManager(ScopeController scope, IEnumerable<ITraceMapper> mappers)
        {
            _scope = scope;
            _pipeline = new TracePipeline(mappers);
            _adapter = new ScopeControllerAdapter(scope);
        }

        public void LoadAll(IEnumerable<LogEntry> entries)
        {
            if (_pipeline == null || _adapter == null)
                return;

            if (!entries.Any())
                return;


            var assigned = _pipeline.Run(entries);
            _adapter.LoadAndReturnTraces(assigned, entries);
            _scope.RedrawAll();
        }
    }
}
