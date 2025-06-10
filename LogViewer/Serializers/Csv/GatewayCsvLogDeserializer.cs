using KC.InternalApi.Model;
using LogViewer.AppContext;
using LogViewer.Logging;
using LogViewer.Providers.API;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LogViewer.Serializers.Csv
{
    public class GatewayCsvLogDeserializer : ICsvDeseralizer
    {
        public LogViewerContext Deserialize(StreamReader reader)
        {
            LogCollection collection = new LogCollection();

            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;

            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();

                if (line == null)
                    continue;

                if (TryParse(line, out var gatewayLog))
                {
                    var entry = WebApiLogItemConverter.ConvertItem(gatewayLog);

                    if (entry != null)
                    {
                        if (start > gatewayLog.TimeStamp)
                            start = gatewayLog.TimeStamp.Value;
                        if (end < gatewayLog.TimeStamp)
                            end = gatewayLog.TimeStamp.Value;
                        collection.Entries.Add(entry);
                    }
                }
            }

            LogViewerContext result = new LogViewerContext();
            result.LogCollection = collection;  
            result.ScopeViewContext.EndDate = end;
            result.ScopeViewContext.StartDate = start;
            return result;
        }



        bool TryParse(string line, out GatewayLog gatewayLog)
        {
            gatewayLog = new GatewayLog();
            string[] split = line.Split(';');

            DateTime timestamp = DateTime.MinValue;
            string xBEEAddress;
            byte[] data;
            string version = "";
            //TODO: This is a problem, don't know how to fix this...


            bool timeSucess = false;

            if (timeSucess == false) timeSucess = DateTime.TryParse(split[0], out timestamp);
            if (timeSucess == false) timeSucess = DateTime.TryParseExact(split[0], "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp);
            if (timeSucess == false) timeSucess = DateTime.TryParseExact(split[0], "d-M-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp);

            if (timestamp < DateTime.ParseExact("01-01-1980", "d-M-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None))
                return false;

            if (timeSucess == false)
                return false;

            Match m = Regex.Match(split[1], @"0x([A-Fa-f\d]+)");
            if (!m.Success)
                return false;

            if (!byte.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte code))
                return false;
            if (!int.TryParse(split[2], out int actionCode))
                return false;
            xBEEAddress = split[3];

            string[] dataSplit = split[4].Trim(' ').Split(' ');
            data = new byte[dataSplit.Length];

            for (int i = 0; i < data.Length; i++)
                if (!byte.TryParse(dataSplit[i].Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out data[i]))
                    return false;

            if (split.Length >= 5)
                version = split[5];

            gatewayLog.ActionCode = actionCode;
            gatewayLog.Code = code;
            gatewayLog.Data = data;
            gatewayLog.TimeStamp = timestamp;
            gatewayLog.VarVersion = version;

            return true;
        }
    }
}






