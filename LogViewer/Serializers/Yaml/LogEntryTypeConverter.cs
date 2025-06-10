using LogViewer.Logging;
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;


namespace LogViewer.Serializers.Yaml
{
    public class LogEntryTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(LogEntry);
        }

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var builder = new LogEntryBuilder();

            // Ensure we're starting at a Mapping
            if (parser.Current is not MappingStart)
            {
                throw new FormatException("Expected start of mapping.");
            }

            // Move past the MappingStart
            parser.MoveNext();

            while (parser.Current is not MappingEnd)
            {
                // Read the key (must be a scalar)
                if (parser.Current is not Scalar keyScalar)
                {
                    throw new FormatException("Expected a key in the mapping.");
                }

                var key = keyScalar.Value;

                // Move to the value
                parser.MoveNext();

                // Read the value (must be a scalar)
                if (parser.Current is not Scalar valueScalar)
                {
                    throw new FormatException("Expected a value in the mapping.");
                }

                var value = valueScalar.Value;

                // Convert the key to LogKeys enum and build the LogEntry
                if (Enum.TryParse<LogKeys>(key, out var logKey))
                {
                    builder.WithSegment(logKey, value);
                }
                else
                {
                    throw new FormatException($"Invalid key format: {key}");
                }

                // Move to the next item in the mapping
                parser.MoveNext();
            }

            // Move past the MappingEnd
            parser.MoveNext();

            return builder.Build();
        }


        public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
        {
            var logEntry = (LogEntry)value;

            emitter.Emit(new MappingStart()); // Start the mapping

            foreach (var kvp in logEntry.Segments)
            {
                // Emit the key as a scalar
                emitter.Emit(new Scalar(kvp.Key.ToString()));
                emitter.Emit(new Scalar(kvp.Value));
            }

            emitter.Emit(new MappingEnd()); // End the mapping
        }

    }
}




