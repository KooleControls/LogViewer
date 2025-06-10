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

        public string ApiUrl { get => apiUrl; set => Do(ref apiUrl, value); }
        public string Host { get => host; set => Do(ref host, value); }
        public string IST { get => iST; set => Do(ref iST, value); }
        public string COM { get => cOM; set => Do(ref cOM, value); }
        public string TRG { get => tRG; set => Do(ref tRG, value); }
        public string SID { get => sID; set => Do(ref sID, value); }
        public string DEVID { get => dEVID; set => Do(ref dEVID, value); }

        public InfoViewManager(RichTextBox textBox)
        {
            this.textBox = textBox;
            Print();
        }



        void Print()
        {
            textBox.Text = @$"Api  {ApiUrl}
Host {Host}
IST     COM     TRG     SID     DEVID
{IST.PadRight(7)} {COM.PadRight(7)} {TRG.PadRight(7)} {SID.PadRight(7)} {DEVID.PadRight(7)}
";
        }


        void Do(ref string sout, string s)
        {
            sout = s;
            Print();
        }




    }
}
