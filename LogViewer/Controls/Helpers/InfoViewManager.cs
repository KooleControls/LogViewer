using LogViewer.Config.Models;
using KC.InternalApi.Model;

namespace LogViewer.Controls.Helpers
{
    public class InfoViewManager
    {
        private readonly RichTextBox textBox;

        private string host = "";
        private string iST = "";
        private string cOM = "";
        private string tRG = "";
        private string sID = "";
        private string dEVID = "";
        private string apiUrl = "";

        public InfoViewManager(RichTextBox textBox)
        {
            this.textBox = textBox;
            Print();
        }

        public void Update(OrganisationConfig? organisationConfig)
        {
            apiUrl = organisationConfig?.BasePath ?? "";
            Print();
        }

        public void Update(ResortSettings? resortSettings)
        {
            iST = resortSettings?.InstallCode ?? "";
            host = resortSettings?.ConnectionServerSettings?.ServerAddress ?? "";
            cOM = resortSettings?.ConnectionServerSettings?.ComPort?.ToString() ?? "";
            tRG = resortSettings?.ConnectionServerSettings?.TrgPort?.ToString() ?? "";
            Print();
        }

        public void Update(Gateway? gateway)
        {
            sID = gateway?.Sid?.ToString() ?? "";
            dEVID = gateway?.GatewayId?.ToString() ?? "";
            Print();
        }

        public void ClearOrganisationInfo()
        {
            apiUrl = "";
            ClearResortInfo();
            Print();
        }

        public void ClearResortInfo()
        {
            host = "";
            iST = "";
            cOM = "";
            tRG = "";
            ClearGatewayInfo();
            Print();
        }

        public void ClearGatewayInfo()
        {
            sID = "";
            dEVID = "";
            Print();
        }



        private void Print()
        {
            textBox.Text = @$"Api  {apiUrl}
Host {host}
IST     COM     TRG     SID     DEVID
{iST.PadRight(7)} {cOM.PadRight(7)} {tRG.PadRight(7)} {sID.PadRight(7)} {dEVID.PadRight(7)}
";
        }
    }
}
