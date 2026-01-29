using LogViewer.Files.Interfaces;

namespace LogViewer.Files.Core
{
    public interface IFileImporterProvider
    {
        bool TryGetImporter(FileFormat format, out IFileImporter importer);
    }





}
