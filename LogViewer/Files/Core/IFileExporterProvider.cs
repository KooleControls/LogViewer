using LogViewer.Files.Interfaces;

namespace LogViewer.Files.Core
{
    public interface IFileExporterProvider
    {
        bool TryGetExporter(FileFormat format, out IFileExporter exporter);
    }





}
