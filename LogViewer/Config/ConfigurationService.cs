using LogViewer.Config.Models;
using System.Reflection;
using LogViewer.Config.Mergers;
using LogViewer.Config.Loaders;
using LogViewer.Config.Resources;
using LogViewer.Config.Schema;
using LogViewer.Config.Hashing;
using LogViewer.Config.Cache;
using LogViewer.Config.Helpers;

namespace LogViewer.Config
{
    public class ConfigurationService
    {

        // Embedded resources

#if DEBUG
        private static readonly string VsCodeSettingsResource = $"{Assembly.GetExecutingAssembly().GetName().Name}.Resources.vscode.settings.json";
        private static readonly string ConfigFileResource = $"{Assembly.GetExecutingAssembly().GetName().Name}.Resources.default_config.development.yaml";

        private static readonly string VsCodeSettingsFile = PathHelper.NormalizePath("%SOLUTION%\\Config\\.vscode\\settings.json");
        private static readonly string SchemaFile =  PathHelper.NormalizePath("%SOLUTION%\\Config\\schema.json");
        private static readonly string ConfigFile =  PathHelper.NormalizePath("%LOCALAPPDATA%\\LogViewer\\debug\\config.yaml");
        private static readonly string CacheFolder = PathHelper.NormalizePath("%LOCALAPPDATA%\\LogViewer\\debug\\cache");

#else
        private static readonly string VsCodeSettingsResource = $"{Assembly.GetExecutingAssembly().GetName().Name}.Resources.vscode.settings.json";
        private static readonly string ConfigFileResource = $"{Assembly.GetExecutingAssembly().GetName().Name}.Resources.default_config.yaml";

        private static readonly string VsCodeSettingsFile =  PathHelper.NormalizePath("%LOCALAPPDATA%\\LogViewer\\release\\.vscode\\settings.json");
        private static readonly string SchemaFile =          PathHelper.NormalizePath("%LOCALAPPDATA%\\LogViewer\\release\\schema.json");
        private static readonly string ConfigFile =          PathHelper.NormalizePath("%LOCALAPPDATA%\\LogViewer\\release\\config.yaml");
        private static readonly string CacheFolder =         PathHelper.NormalizePath("%LOCALAPPDATA%\\LogViewer\\release\\cache");
#endif

        private readonly IResourceProvider _resourceProvider;
        private readonly ISchemaBuilder _schemaBuilder;
        private readonly IConfigLoader _configLoader;
        private readonly IConfigUpdateChecker _configUpdateChecker;

        public ConfigurationService()
        {
            // Shared infrastructure
            var hashProvider = new Md5HashProvider();
            var cacheManager = new CacheManager(CacheFolder, hashProvider);
            var merger = BuildMerger();

            _resourceProvider = new ResourceProvider();
            _schemaBuilder = new SchemaBuilder();
            _configLoader = new RecursiveConfigLoader(cacheManager, merger);
            _configUpdateChecker = new RecursiveConfigUpdateChecker(cacheManager);

            EnsureDefaultConfigExists();
            EnsureSchemaExists();
            EnsureVsCodeSettingsExists();
        }

        private static IConfigMerger BuildMerger()
        {
            var merger = new ConfigMerger();
            merger.RegisterMerger(new LogViewerConfigMerger());
            merger.RegisterMerger(new DictionaryMerger());
            merger.RegisterMerger(new ObjectMerger());
            return merger;
        }

        public async Task<LogViewerConfig> GetConfigAsync(CancellationToken token = default)
        {
            return await _configLoader.LoadAndMergeAsync(ConfigFile, token);
        }

        public async Task<bool> DownloadIfUpdatedAsync(CancellationToken token = default)
        {
            return await _configUpdateChecker.DownloadIfUpdatedRecursiveAsync(ConfigFile, token);
        }

        public void EnsureDefaultConfigExists()
        {
            if (File.Exists(ConfigFile))
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(ConfigFile)!);
            var fileContents = _resourceProvider.ReadEmbeddedYaml(ConfigFileResource);
            File.WriteAllText(ConfigFile, fileContents);

            Directory.CreateDirectory(Path.GetDirectoryName(SchemaFile)!);
        }

        public void EnsureVsCodeSettingsExists()
        {
            if (File.Exists(VsCodeSettingsFile))
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(VsCodeSettingsFile)!);
            var fileContents = _resourceProvider.ReadEmbeddedYaml(VsCodeSettingsResource);
            File.WriteAllText(VsCodeSettingsFile, fileContents);
        }

        public void EnsureSchemaExists()
        {
            var newSchema = _schemaBuilder.GetSchema();

            if (File.Exists(SchemaFile))
            {
                var existingSchema = File.ReadAllText(SchemaFile);

                if (existingSchema == newSchema)
                    return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(SchemaFile)!);
            File.WriteAllText(SchemaFile, newSchema);
        }
    }

}
