using LogViewer.AppContext;
using LogViewer.Files.Core;

namespace LogViewer.Files
{
    public sealed class LogViewerFileService
    {
        private readonly IFileImportExportProvider _provider;
        private readonly IFileFormatDetector _detector;

        public LogViewerFileService(IFileImportExportProvider provider, IFileFormatDetector detector)
        {
            _provider = provider;
            _detector = detector;
        }

        public bool TryLoad(FileInfo file, out LogViewerContext context)
        {
            var format = _detector.DetectFormat(file);

            if (!_provider.TryGetImporter(format, out var importer))
            {
                context = default!;
                return false;
            }

            using var fs = file.OpenRead();
            return importer.TryImport(fs, out context);
        }

        public void Save(FileInfo file, LogViewerContext context)
        {
            var format = _detector.DetectFormat(file); // or detect by extension only for save

            if (!_provider.TryGetExporter(format, out var exporter))
                throw new NotSupportedException($"Export not supported for {format}.");

            using var fs = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
            exporter.Export(fs, context);
        }

        public bool TryLoad(Stream stream, FileFormat format, out LogViewerContext context)
        {
            if (!_provider.TryGetImporter(format, out var importer))
            {
                context = default!;
                return false;
            }

            return importer.TryImport(stream, out context);
        }

        public void Save(Stream stream, LogViewerContext context, FileFormat format)
        {
            if (!_provider.TryGetExporter(format, out var exporter))
                throw new NotSupportedException($"Export not supported for {format}.");

            exporter.Export(stream, context);
        }
    }





}
