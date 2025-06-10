using LogViewer.Config.Hashing;
using System.Diagnostics;

namespace LogViewer.Config.Cache
{

    public class CacheManager : ICacheManager
    {
        private readonly string _cacheFolder;
        private readonly IHashProvider _hashProvider;

        public CacheManager(string cacheFolder, IHashProvider hashProvider)
        {
            _cacheFolder = cacheFolder;
            _hashProvider = hashProvider;
        }

        public string GetCacheFilePath(string url)
        {
            string extention = Path.GetExtension(url);
            string hash = _hashProvider.ComputeHash(url);
            return Path.Combine(_cacheFolder, $"{hash}{extention}");
        }

        public async Task<bool> DownloadIfUpdatedAsync(string url, CancellationToken token = default)
        {
            string path = GetCacheFilePath(url);

            using var buffer = new MemoryStream();
            string? newHash = await DownloadFileToBufferAsync(url, buffer, token);
            if (newHash == null)
                return false;

            string? existingHash = await GetFileHashAsync(path, token);
            if (existingHash == newHash)
                return false;

            Directory.CreateDirectory(_cacheFolder);
            buffer.Position = 0;

            using var file = new FileStream(path, FileMode.Create, FileAccess.Write);
            await buffer.CopyToAsync(file, token);
            return true;
        }

        private async Task<string?> DownloadFileToBufferAsync(string url, Stream buffer, CancellationToken token)
        {
            try
            {
                using var client = new HttpClient();
                using var stream = await client.GetStreamAsync(url, token);
                return await _hashProvider.CopyAndHashAsync(stream, buffer, token);
            }
            catch
            {
                return null;
            }
        }

        private async Task<string?> GetFileHashAsync(string path, CancellationToken token)
        {
            if (!File.Exists(path)) return null;

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return await _hashProvider.ComputeHashAsync(stream, token);
        }
    }
}
