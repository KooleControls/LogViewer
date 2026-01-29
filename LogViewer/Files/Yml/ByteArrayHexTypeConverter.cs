using System.Globalization;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;


namespace LogViewer.Files.Yml
{
    public class ByteArrayHexTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(byte[]);
        }

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var hexString = parser.Consume<Scalar>().Value;
            if (hexString.StartsWith("#"))
                hexString = hexString.Substring(1);
            return HexStringToByteArray(hexString);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
        {
            var byteArray = (byte[])value;
            var hexString = "#" + ByteArrayToHexString(byteArray);
            emitter.Emit(new Scalar(hexString));
        }

        private static string ByteArrayToHexString(byte[] byteArray)
        {
            var stringBuilder = new StringBuilder(byteArray.Length * 2);
            foreach (var b in byteArray)
            {
                stringBuilder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }
            return stringBuilder.ToString();
        }

        private static byte[] HexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("Hex string must have an even length");

            var byteArray = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return byteArray;
        }
    }
}




