using LogViewer.AppContext;
using LogViewer.Files.Core;
using LogViewer.Files.Interfaces;
using System.IO.Compression;

namespace LogViewer.Files.Json
{
    public sealed class GzFileImporter : IFileImporter
    {
        public FileFormat Format => FileFormat.Gz;

        private readonly JsonFileImporter _jsonImporter;

        public GzFileImporter()
        {
            _jsonImporter = new JsonFileImporter();
        }

        public bool TryImport(Stream stream, out LogViewerContext context)
        {
            try
            {
                // Caller owns 'stream' => leaveOpen: true
                using var gz = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
                return _jsonImporter.TryImport(gz, out context);
            }
            catch
            {
                context = default!;
                return false;
            }
        }
    }
}
