namespace LogViewer.Config.Cache
{
    public interface ICacheManager
    {
        /// <summary>
        /// Returns the expected cache file path for a given URL.
        /// Does NOT check if the file exists.
        /// </summary>
        string GetCacheFilePath(string url);

        /// <summary>
        /// Downloads the file if needed.
        /// Returns true if the file was newly downloaded or updated.
        /// </summary>
        Task<bool> DownloadIfUpdatedAsync(string url, CancellationToken token = default);
    }

}

