namespace LogViewer.Config.Loaders
{
    public interface IConfigUpdateChecker
    {
        Task<bool> DownloadIfUpdatedRecursiveAsync(string entryPath, CancellationToken token = default);
    }
}