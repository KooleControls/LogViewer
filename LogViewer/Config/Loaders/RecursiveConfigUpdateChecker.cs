using LogViewer.Config.Cache;
using LogViewer.Config.Helpers;
using LogViewer.Config.Models;
using LogViewer.Serializers.Yaml;
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
            return await CheckRecursiveAsync(entryPath, visited, token);
        }

        private async Task<bool> CheckRecursiveAsync(string path, HashSet<string> visited, CancellationToken token)
        {
            bool updated = false;

            var file = PathHelper.ReplaceWildcards(path);

            if (visited.Contains(file))
                return false;

            if (!visited.Add(file))
                return false;

            if (PathHelper.IsRemotePath(file))
            {
                updated = await _cacheManager.DownloadIfUpdatedAsync(file, token);
                file = _cacheManager.GetCacheFilePath(file);
            }
            
            if (!File.Exists(file))
                return updated;

            if (!YamlSerializer.LoadYaml(new FileInfo(file), out LogViewerConfig config))
                return updated;

            string basePath = PathHelper.GetBasePath(path);

            foreach (var source in config.Sources.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                string resolved = PathHelper.IsPathRooted(source)
                    ? source
                    : PathHelper.CombinePaths(basePath, source);

                updated |= await CheckRecursiveAsync(resolved, visited, token);
            }

            return updated;
        }
    }
}