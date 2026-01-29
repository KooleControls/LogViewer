using LogViewer.Files.Core;

namespace LogViewer.Files
{
    public sealed class FileFormatDetector : IFileFormatDetector
    {
        public FileFormat DetectFormat(FileInfo file)
        {
            var ext = file.Extension.ToLowerInvariant();

            return ext switch
            {
                ".yaml" or ".yml" => FileFormat.Yaml,
                ".cyaml" => FileFormat.Cyaml,
                ".json" => FileFormat.Json,
                ".gz" => FileFormat.Gz,
                ".csv" => FileFormat.Csv,
                _ => throw new NotSupportedException($"Unknown file extension: {file.Extension}")
            };
        }
    }

}
