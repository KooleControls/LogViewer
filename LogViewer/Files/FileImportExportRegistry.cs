using LogViewer.Files.Core;
using LogViewer.Files.Interfaces;

namespace LogViewer.Files
{
    public sealed class FileImportExportRegistry : IFileImportExportProvider
    {
        private readonly Dictionary<FileFormat, IFileImporter> _importers = new();
        private readonly Dictionary<FileFormat, IFileExporter> _exporters = new();

        public FileImportExportRegistry AddImporter(IFileImporter importer)
        {
            _importers[importer.Format] = importer;
            return this;
        }

        public FileImportExportRegistry AddExporter(IFileExporter exporter)
        {
            _exporters[exporter.Format] = exporter;
            return this;
        }

        public bool TryGetImporter(FileFormat format, out IFileImporter importer)
            => _importers.TryGetValue(format, out importer!);

        public bool TryGetExporter(FileFormat format, out IFileExporter exporter)
            => _exporters.TryGetValue(format, out exporter!);
    }
}
