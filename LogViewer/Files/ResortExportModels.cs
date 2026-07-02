using System.Collections.Generic;
using System.Xml.Serialization;

namespace LogViewer.Files
{
    // Bestandsformaat van een resort-export (.kcresort) dat de KC220 config tool importeert.
    // Bewust neutraal en "dom": alle beschikbare API-velden staan erin; de mapping (welke poort,
    // host-afleiding, encryptie) gebeurt bij de import in de 220 tool. LET OP: de volgorde en namen
    // van de leden moeten gelijk blijven aan KcResortExport in de 220 tool (XmlSerializer).
    [XmlRoot("KcResortExport")]
    public class ResortExport
    {
        public string? Organisation { get; set; }
        public string? OrganisationHost { get; set; }
        public string? ResortName { get; set; }
        public string? ServerAddress { get; set; }
        public string? ComPort { get; set; }
        public string? TrgPort { get; set; }
        public string? InstallCode { get; set; }

        [XmlArray("Devices")]
        [XmlArrayItem("Device")]
        public List<ResortExportDevice> Devices { get; set; } = new();
    }

    public class ResortExportDevice
    {
        public string? Name { get; set; }
        public int Sid { get; set; }        // Software ID  (Gateway.Sid)
        public int DeviceId { get; set; }   // Device ID    (Gateway.GatewayId)
    }
}
