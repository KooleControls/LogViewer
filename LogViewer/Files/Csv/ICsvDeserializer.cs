using LogViewer.AppContext;

namespace LogViewer.Files.Csv
{
    public interface ICsvDeserializer
    {
        LogViewerContext Deserialize(StreamReader reader);
    }
}

