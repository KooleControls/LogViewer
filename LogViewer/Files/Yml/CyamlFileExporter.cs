using LogViewer.AppContext;
using LogViewer.Files.Core;
using LogViewer.Files.Interfaces;
using System.IO.Compression;

namespace LogViewer.Files.Yml
{
    public sealed class CyamlFileExporter : IFileExporter
    {
        public FileFormat Format => FileFormat.Cyaml;

        private readonly YamlFileExporter _yamlExporter;

        public CyamlFileExporter()
        {
            _yamlExporter = new YamlFileExporter();
        }

        public void Export(Stream stream, LogViewerContext context)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);

            var entry = archive.CreateEntry("data.yaml", CompressionLevel.Optimal);

            using var entryStream = entry.Open();
            _yamlExporter.Export(entryStream, context);
        }
    }
}

