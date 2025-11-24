using FormsLib.Scope;

namespace LogViewer.Mapping
{
    public class BuiltTrace
    {
        public Trace Trace { get; set; } = null!;
        public List<FormsLib.Scope.Label> Labels { get; set; } = new();
    }
}
