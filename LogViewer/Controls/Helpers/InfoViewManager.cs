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

        private int completedRequests = 0;
        private TimeSpan minResponseTime = TimeSpan.MaxValue;
        private TimeSpan maxResponseTime = TimeSpan.Zero;
        private TimeSpan totalResponseTime = TimeSpan.Zero;
        private TimeSpan avgResponseTime = TimeSpan.Zero;

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

        public void ReportApiCall(TimeSpan elapsed)
        {
            completedRequests++;
            totalResponseTime += elapsed;

            if (elapsed < minResponseTime)
                minResponseTime = elapsed;

            if (elapsed > maxResponseTime)
                maxResponseTime = elapsed;

            avgResponseTime = totalResponseTime / completedRequests;

            Print();
        }

        public void ClearApiStatsInfo()
        {
            completedRequests = 0;
            minResponseTime = TimeSpan.MaxValue;
            maxResponseTime = TimeSpan.Zero;
            totalResponseTime = TimeSpan.Zero;
            avgResponseTime = TimeSpan.Zero;
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
            string compl = this.completedRequests.ToString();
            string min = minResponseTime != TimeSpan.MaxValue ? $"{minResponseTime.TotalMilliseconds:F0} ms" : "-";
            string max = maxResponseTime != TimeSpan.Zero ? $"{maxResponseTime.TotalMilliseconds:F0} ms" : "-";
            string avg = completedRequests > 0 ? $"{avgResponseTime.TotalMilliseconds:F0} ms" : "-";

            textBox.Text = @$"Api  {apiUrl}
Host {host}
IST     COM     TRG     SID     DEVID
{iST.PadRight(7)} {cOM.PadRight(7)} {tRG.PadRight(7)} {sID.PadRight(7)} {dEVID.PadRight(7)}
Calls   Min     Max     Avg
{compl.PadRight(7)} {min.PadRight(7)} {max.PadRight(7)} {avg.PadRight(7)}
";

        }
    }
}
