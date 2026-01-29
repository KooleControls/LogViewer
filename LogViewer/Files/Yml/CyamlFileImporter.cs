using LogViewer.AppContext;
using LogViewer.Files.Core;
using LogViewer.Files.Interfaces;

namespace LogViewer.Files.Yml
{
    public sealed class CyamlFileImporter : IFileImporter
    {
        public FileFormat Format => FileFormat.Cyaml;
        private readonly YamlFileImporter _yamlImporter;
        public CyamlFileImporter()
        {
            _yamlImporter = new YamlFileImporter();
        }
        public bool TryImport(Stream stream, out LogViewerContext context)
        {
            try
            {
                using var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read, leaveOpen: true);
                var entry = archive.GetEntry("data.yaml");
                if (entry == null)
                {
                    context = default!;
                    return false;
                }
                using var entryStream = entry.Open();
                return _yamlImporter.TryImport(entryStream, out context);
            }
            catch
            {
                context = default!;
                return false;
            }
        }
    }

}

