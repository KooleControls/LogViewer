using System.Security.Cryptography;
using System.Text;

namespace LogViewer.Config.Hashing
{
    public class Md5HashProvider : IHashProvider
    {
        public string ComputeHash(string input)
        {
            using var md5 = MD5.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(bytes);
            return ToHexString(hash);
        }

        public async Task<string> ComputeHashAsync(Stream stream, CancellationToken token)
        {
            using var md5 = MD5.Create();
            byte[] buffer = new byte[4096];
            int read;

            while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token)) > 0)
                md5.TransformBlock(buffer, 0, read, buffer, 0);

            md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return ToHexString(md5.Hash!);
        }

        public async Task<string> CopyAndHashAsync(Stream source, Stream destination, CancellationToken token)
        {
            using var md5 = MD5.Create();
            byte[] buffer = new byte[4096];
            int read;

            while ((read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), token)) > 0)
            {
                md5.TransformBlock(buffer, 0, read, buffer, 0);
                await destination.WriteAsync(buffer.AsMemory(0, read), token);
            }

            md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return ToHexString(md5.Hash!);
        }

        private string ToHexString(byte[] hash) =>
            BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}