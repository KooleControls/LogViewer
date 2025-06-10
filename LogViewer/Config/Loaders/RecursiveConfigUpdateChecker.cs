using LogViewer.Config.Cache;
using LogViewer.Config.Helpers;
using LogViewer.Config.Models;
using LogViewer.Serializers.Yaml;
using System.Diagnostics;
using System.IO;

namespace LogViewer.Config.Loaders
{
    public class RecursiveConfigUpdateChecker : IConfigUpdateChecker
    {
        private readonly ICacheManager _cacheManager;

        public RecursiveConfigUpdateChecker(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task<bool> DownloadIfUpdatedRecursiveAsync(string entryPath, CancellationToken token = default)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            entryPath = PathHelper.NormalizePath(entryPath);
            return await CheckRecursiveAsync(entryPath, visited, token);
        }

        private async Task<bool> CheckRecursiveAsync(string orig, HashSet<string> visited, CancellationToken token)
        {
            bool updated = false;

            string path = orig;

            if (visited.Contains(path))
                return false;

            if (!visited.Add(path))
                return false;

            if (PathHelper.IsRemotePath(path))
            {
                updated = await _cacheManager.DownloadIfUpdatedAsync(path, token);
                path = _cacheManager.GetCacheFilePath(path);
            }
            
            if (!File.Exists(path))
                return updated;

            if (!YamlSerializer.LoadYaml(new FileInfo(path), out LogViewerConfig config))
                return updated;

            string basePath = PathHelper.GetBasePath(orig);

            foreach (var source in config.Sources.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                var normalizedSource = PathHelper.NormalizePath(source);

                string resolved = PathHelper.IsPathRooted(normalizedSource)
                    ? normalizedSource
                    : PathHelper.CombinePaths(basePath, normalizedSource);

                var normalizedResolved = PathHelper.NormalizePath(resolved);
                updated |= await CheckRecursiveAsync(normalizedResolved, visited, token);
            }

            return updated;
        }
    }
}