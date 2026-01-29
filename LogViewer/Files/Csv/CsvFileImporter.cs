using LogViewer.AppContext;
using LogViewer.Files.Core;
using LogViewer.Files.Interfaces;
using System.Reflection.PortableExecutable;
using System.Text;

namespace LogViewer.Files.Csv
{
    public sealed class CsvFileImporter : IFileImporter
    {
        public FileFormat Format => FileFormat.Csv;

        public bool TryImport(Stream stream, out LogViewerContext context)
        {
            context = default!;

            try
            {
                // Caller owns stream lifetime => leaveOpen: true
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

                var deserializer = CreateDeserializer(reader);
                if (deserializer == null)
                    return false;

                // IMPORTANT: CreateDeserializer consumed the first line (header),
                // so the deserializer reads from current position (same as old code)
                context = deserializer.Deserialize(reader);
                return context != null;
            }
            catch
            {
                context = default!;
                return false;
            }
        }

        private static ICsvDeserializer? CreateDeserializer(StreamReader rdr)
        {
            var firstLine = rdr.ReadLine() ?? "";
            string[] columns = firstLine.ToLowerInvariant().Split(';');

            // Same crude detection as before, just relocated
            if (ContainsAll("Timestamp;Code;Actioncode;XBee adres;Data", columns))
                return new GatewayCsvLogDeserializer();

            return null;
        }

        private static bool ContainsAll(string first, string[] second)
        {
            return Array.TrueForAll(
                first.ToLowerInvariant().Split(';'),
                a => second.Contains(a));
        }
    }
}

