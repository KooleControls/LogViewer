using LogViewer.AppContext;
using LogViewer.Logging;

namespace LogViewer.Serializers.Csv
{
    public interface ICsvDeseralizer
    {
        LogViewerContext Deserialize(StreamReader reader);
    }
}






