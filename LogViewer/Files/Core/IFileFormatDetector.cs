namespace LogViewer.Files.Core
{
    public interface IFileFormatDetector
    {
        FileFormat DetectFormat(FileInfo file);
    }
}
