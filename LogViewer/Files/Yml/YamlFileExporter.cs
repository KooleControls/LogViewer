using LogViewer.AppContext;
using LogViewer.Files.Core;
using LogViewer.Files.Interfaces;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LogViewer.Files.Yml
{
    public sealed class YamlFileExporter : IFileExporter
    {
        public FileFormat Format => FileFormat.Yaml;
        private readonly ISerializer _serializer;
        public YamlFileExporter()
        {
            _serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new ColorTypeConverter())
                .WithTypeConverter(new ByteArrayHexTypeConverter())
                //.WithTypeConverter(new LogEntryTypeConverter())
                .Build();
        }


        public void Export(Stream stream, LogViewerContext context)
        {
            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
            _serializer.Serialize(writer, context);
            writer.Flush();
        }
    }
}

