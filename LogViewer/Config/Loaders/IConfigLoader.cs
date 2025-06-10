using LogViewer.Config.Models;

namespace LogViewer.Config.Loaders
{
    public interface IConfigLoader
    {
        Task<LogViewerConfig> LoadAndMergeAsync(string entryPath, CancellationToken token = default);
    }
}