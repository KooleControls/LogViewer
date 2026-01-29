using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;


namespace LogViewer.Files.Yml
{
    public class ColorTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(Color);
        }

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var stringValue = parser.Consume<Scalar>().Value;

            if (stringValue.StartsWith("0x"))
                stringValue = stringValue.Substring(2);

            if (stringValue.StartsWith("#"))
                stringValue = stringValue.Substring(1);

            if (uint.TryParse(stringValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var colorValue))
            {
                return Color.FromArgb(
                    (int)(colorValue >> 24 & 0xFF),
                    (int)(colorValue >> 16 & 0xFF),
                    (int)(colorValue >> 8 & 0xFF),
                    (int)(colorValue & 0xFF)
                );
            }
            throw new FormatException("Invalid uint64 color format");
        }

        public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
        {
            var color = (Color)value;
            uint colorValue =
                (uint)color.A << 24 |
                (uint)color.R << 16 |
                (uint)color.G << 8 |
                 color.B;

            emitter.Emit(new Scalar($"#{colorValue.ToString("X4", CultureInfo.InvariantCulture)}"));
        }
    }
}




