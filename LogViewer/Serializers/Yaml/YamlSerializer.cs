using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeResolvers;
using YamlDotNet.Serialization.Utilities;


namespace LogViewer.Serializers.Yaml
{
    public static class YamlSerializer
    {
        static public bool LoadYaml<T>(FileInfo file, out T obj)
        {
            obj = default;
            if (!file.Exists)
                return false;

            try
            {
                using var reader = new StreamReader(file.OpenRead());
                var deserializer = CreateDeserializer();
                var deserialized = deserializer.Deserialize<T>(reader);
                if (deserialized == null)
                    return false;
                obj = deserialized;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
            return true;
        }

        static public void SaveYaml<T>(FileInfo file, T obj)
        {
            string? path = Path.GetDirectoryName(file.FullName);
            if (path != null)
                Directory.CreateDirectory(path);


            var serializer = CreateSerializer();
            using var writer = new StreamWriter(file.Open(FileMode.Create, FileAccess.Write));
            serializer.Serialize(writer, obj);
        }


        static public bool LoadYamlZip<T>(FileInfo file, out T obj)
        {
            obj = default;
            if (!file.Exists)
                return false;

            var deserializer = CreateDeserializer();
            using var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
            var entry = archive.GetEntry("data.yaml"); // Use the name that was used in SaveYamlGZip
            if (entry == null)
                return false;

            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream, Encoding.UTF8);
            var deserialized = deserializer.Deserialize<T>(reader);
            obj = deserialized;
            return deserialized != null;
        }

        static public void SaveYamlZip<T>(FileInfo file, T obj)
        {
            string? path = Path.GetDirectoryName(file.FullName);
            if (path != null)
                Directory.CreateDirectory(path);

            // Create a serializer
            var serializer = CreateSerializer();


            // Write YAML data to a zip file
            using var fileStream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write);
            using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true);
            var entry = archive.CreateEntry("data.yaml"); // or use the file name without extension
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream, Encoding.UTF8);
            serializer.Serialize(writer, obj);
        }

        public static ISerializer CreateSerializer()
        {
            return new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new ColorTypeConverter())
                .WithTypeConverter(new ByteArrayHexTypeConverter())
                //.WithTypeConverter(new LogEntryTypeConverter())
                .Build();
        }


        public static IDeserializer CreateDeserializer()
        {
            return new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new ColorTypeConverter())
                .WithTypeConverter(new ByteArrayHexTypeConverter())
                //.WithTypeConverter(new LogEntryTypeConverter())
                .Build();
        }
    }
}




