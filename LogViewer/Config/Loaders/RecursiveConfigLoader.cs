using LogViewer.Config.Models;
using LogViewer.Config.Mergers;
using LogViewer;
using LogViewer.Config.Cache;
using LogViewer.Config.Helpers;
using LogViewer.Serializers.Yaml;

namespace LogViewer.Config.Loaders
{
    public class RecursiveConfigLoader : IConfigLoader
    {
        private readonly ICacheManager _cacheManager;
        private readonly IConfigMerger _merger;

        public RecursiveConfigLoader(ICacheManager cacheManager, IConfigMerger merger)
        {
            _cacheManager = cacheManager;
            _merger = merger;
        }

        public async Task<LogViewerConfig> LoadAndMergeAsync(string entryPath, CancellationToken token = default)
        {
            var result = new LogViewerConfig();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await LoadRecursiveAsync(entryPath, result, visited, token);
            return result;
        }

        private async Task LoadRecursiveAsync(string path, LogViewerConfig target, HashSet<string> visited, CancellationToken token)
        {
            path = PathHelper.ReplaceWildcards(path);

            if (visited.Contains(path))
                return;

            if (!visited.Add(path))
                return;

            var filePath = PathHelper.IsRemotePath(path)
                ? _cacheManager.GetCacheFilePath(path)
                : path;

            if (!File.Exists(filePath))
                return;

            if (!YamlSerializer.LoadYaml(new FileInfo(filePath), out LogViewerConfig config))
                return;

            string basePath = PathHelper.GetBasePath(path);

            foreach (var source in config.Sources.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                string resolved = PathHelper.IsPathRooted(source)
                    ? source
                    : PathHelper.CombinePaths(basePath, source);

                await LoadRecursiveAsync(resolved, config, visited, token);
            }

            _merger.TryMerge(target, config);
        }
    }

}