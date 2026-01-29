using LogViewer.AppContext;
using LogViewer.Files.Core;
using LogViewer.Files.Interfaces;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LogViewer.Files.Yml
{
    public sealed class YamlFileImporter : IFileImporter
    {
        public FileFormat Format => FileFormat.Yaml;

        private readonly IDeserializer _deserialiser;

        public YamlFileImporter()
        {
            _deserialiser = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new ColorTypeConverter())
                .WithTypeConverter(new ByteArrayHexTypeConverter())
                //.WithTypeConverter(new LogEntryTypeConverter())
                .Build();
        }

        public bool TryImport(Stream stream, out LogViewerContext context)
        {
            try
            {
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
                context = _deserialiser.Deserialize<LogViewerContext>(reader);
                return true;
            }
            catch
            {
                context = default!;
                return false;
            }
        }
    }

}

