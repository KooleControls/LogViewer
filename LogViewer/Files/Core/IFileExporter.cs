using LogViewer.AppContext;
using LogViewer.Files.Core;

namespace LogViewer.Files.Interfaces
{
    public interface IFileExporter
    {
        FileFormat Format { get; }
        void Export(Stream stream, LogViewerContext context);
    }
}

