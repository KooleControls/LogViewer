using KC.InternalApi.Model;
using LogViewer.AppContext;
using LogViewer.Devices.Gateway;
using LogViewer.Files.Core;
using LogViewer.Files.Interfaces;
using LogViewer.Logging;

namespace LogViewer.Files.Json
{
    public sealed class JsonFileImporter : IFileImporter
    {
        public FileFormat Format => FileFormat.Json;
        public bool TryImport(Stream stream, out LogViewerContext context)
        {
            try
            {
                using var reader = new StreamReader(stream, leaveOpen: true);
                var json = reader.ReadToEnd();
                var jsonContext = System.Text.Json.JsonSerializer.Deserialize<JsonSavedContext>(json);
                if (jsonContext == null)
                {
                    context = default!;
                    return false;
                }
                return TryConvert(jsonContext, out context);
            }
            catch
            {
                context = default!;
                return false;
            }
        }

        private bool TryConvert(JsonSavedContext jsonContext, out LogViewerContext context)
        {
            context = new LogViewerContext();
            context.ScopeViewContext.StartDate = jsonContext.StartDate;
            context.ScopeViewContext.EndDate = jsonContext.EndDate;
            foreach (var jsonEntry in jsonContext.LogItems)
            {

                if (!TryConvertEntry(jsonEntry, out var gwLog))
                    continue;

                var entry = GatewayLogConverter.FromGatewayLog(gwLog);
                if (entry == null)
                    continue;

                context.LogCollection.Entries.Add(entry);
                
            }
            return true;
        }

        private bool TryConvertEntry(JsonSavedLogEntry jsonEntry, out GatewayLog entry)
        {
            entry = new GatewayLog
            {
                ActionCode = jsonEntry.ActionCode,
                Code = jsonEntry.Code,
                Data = jsonEntry.Data,
                TimeStamp = jsonEntry.Timestamp,
            };
            return true;
        }
    }


    public class JsonSavedLogEntry
    {
        public DateTime Timestamp { get; set; } 
        public byte Code { get; set; }
        public byte ActionCode { get; set; }
        public byte[] Data { get; set; } = [];
    }

    public class JsonSavedContext
    {
        public List<JsonSavedLogEntry> LogItems { get; set; } = [];
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }


}

