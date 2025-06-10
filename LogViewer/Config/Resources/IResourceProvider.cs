namespace LogViewer.Config.Resources
{
    public interface IResourceProvider
    {
        string ReadEmbeddedYaml(string resourceName);
    }
}