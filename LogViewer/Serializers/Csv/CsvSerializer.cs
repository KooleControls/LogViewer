using LogViewer.AppContext;
using LogViewer.Logging;


namespace LogViewer.Serializers.Csv
{
    public static class CsvSerializer
    {

        static public bool LoadCsv(FileInfo file, out LogViewerContext obj)
        {
            obj = default;
            if (!file.Exists)
                return false;

            using var reader = new StreamReader(file.OpenRead());
            var deserializer = CreateDeserializer(reader);
            var deserialized = deserializer?.Deserialize(reader);
            if(deserialized == null)
                return false;
            obj = deserialized;
            return true;
        }

        static private ICsvDeseralizer? CreateDeserializer(StreamReader rdr)                       //This is all a bit iffy, make changes where required
        {
            var firstLine = rdr.ReadLine() ?? "";                                   //Assume first line always contain the column names.
            string[] columns = firstLine.ToLower().Split(';');                      //Assume seperator char is always ';'


            //This detection of what parser to use is VERY CRUDE!!! 
            if (ContainsAll("Timestamp;Code;Actioncode;XBee adres;Data", columns))
                return new GatewayCsvLogDeserializer();

            //if (ContainsAll("Timestamp;Code;Version;actionCode;cardTypeX;CustomerType;CardType;CardID;StartTime;EndTime;ReaderType;HotelCode;BatVoltage;CID;CID mode;Antenne", columns)
            //    || ContainsAll("Timestamp;Code;CardID;CardID RAW;Version;actionCode;cardTypeX;CustomerType;CardType;StartTime;EndTime;ReaderType;HotelCode;BatVoltage;CID;CID mode;Antenne", columns))
            //    return new LogItem_X08_CSVParser(columns);

            return null;
        }

        static bool ContainsAll(string first, string[] second)
        {
            return Array.TrueForAll(first.ToLower().Split(';'), a => second.Contains(a));
        }
    }
}






