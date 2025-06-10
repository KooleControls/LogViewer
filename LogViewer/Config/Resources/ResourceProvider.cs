using System.Reflection;

namespace LogViewer.Config.Resources
{
    public class ResourceProvider : IResourceProvider
    {
        public string ReadEmbeddedYaml(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Resource not found: {resourceName}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}