using LogViewer.AppContext;
using LogViewer.Files.Core;

namespace LogViewer.Files.Interfaces
{
    public interface IFileImporter
    {
        FileFormat Format { get; }
        bool TryImport(Stream stream, out LogViewerContext context);
    }





}
