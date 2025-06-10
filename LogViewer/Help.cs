using LogViewer.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer
{
    static class Help
    {
        public static void ShowHelp()
        {
            string url = "https://github.com/KooleControls/LogViewerConfig";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
