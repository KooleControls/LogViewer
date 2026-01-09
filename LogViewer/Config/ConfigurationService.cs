using LogViewer.Config.Helpers;
using LogViewer.Config.Mergers;
using LogViewer.Config.Models;
using LogViewer.Serializers.Yaml;
using System.Reflection;

namespace LogViewer.Config
{
    public class ConfigurationService
    {
#if DEBUG
        private static readonly string ConfigFolder = PathHelper.NormalizePath("%LOCALAPPDATA%\\LogViewer_debug");
#else
    private static readonly string ConfigFolder = PathHelper.NormalizePath("%LOCALAPPDATA%\\LogViewer");
#endif

        private static readonly Assembly ThisAssembly = Assembly.GetExecutingAssembly();
        private static readonly string Root = ThisAssembly.GetName().Name!;

        // Embedded resource names (verify via GetManifestResourceNames)
        private static readonly string VsCodeSettingsResource = $"{Root}.Resources..vscode.settings.json";
        private static readonly string Organisations_SchemaResource = $"{Root}.Resources.definitions.organisations_schema.json";
        private static readonly string System_OrganisationsResource = $"{Root}.Resources.system_organisations.yaml";
        private static readonly string User_OrganisationsResource = $"{Root}.Resources.user_organisations.yaml";

        private static readonly string VsCodeSettingsFile = PathHelper.CombinePaths(ConfigFolder, ".vscode\\settings.json");
        private static readonly string Organisations_SchemaFile = PathHelper.CombinePaths(ConfigFolder, "definitions\\organisations_schema.json");
        private static readonly string User_OrganisationsFile = PathHelper.CombinePaths(ConfigFolder, "user_organisations.yaml");

        private readonly IConfigMerger configMerger;

        public ConfigurationService()
        {
            ExportResourceIfNotExists(VsCodeSettingsResource, VsCodeSettingsFile);
            ExportResourceIfNotExists(Organisations_SchemaResource, Organisations_SchemaFile);
            ExportResourceIfNotExists(User_OrganisationsResource, User_OrganisationsFile);
            configMerger = BuildMerger();
        }

        public LogViewerConfig GetConfigAsync(CancellationToken token = default)
        {
            LogViewerConfig config = new LogViewerConfig();
            configMerger.TryMerge(config, GetConfigFromResource(System_OrganisationsResource));
            configMerger.TryMerge(config, GetConfigFromFile(User_OrganisationsFile));
            return config;
        }

        private LogViewerConfig? GetConfigFromResource(string resource)
        {
            string? yaml = ReadEmbeddedResource(resource);
            if (yaml == null)
                return null;
            if (!YamlSerializer.LoadYaml(yaml, out LogViewerConfig config))
                return null;
            return config;
        }

        private LogViewerConfig? GetConfigFromFile(string filePath)
        {
            if (!YamlSerializer.LoadYaml(new FileInfo(filePath), out LogViewerConfig config))
                return null;
            return config;
        }

        private static void ExportResourceIfNotExists(string resource, string destination)
        {
            string path = Path.GetDirectoryName(destination)!;
            Directory.CreateDirectory(path);
            if (File.Exists(destination))
                return;
            string? content = ReadEmbeddedResource(resource);
            if (content == null)
                return;
            File.WriteAllText(destination, content);
        }

        private static IConfigMerger BuildMerger()
        {
            var merger = new ConfigMerger();
            merger.RegisterMerger(new LogViewerConfigMerger());
            merger.RegisterMerger(new DictionaryMerger());
            merger.RegisterMerger(new ObjectMerger());
            return merger;
        }


        public static string? ReadEmbeddedResource(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null)
                return null;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
