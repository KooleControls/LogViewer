using LogViewer.Logging;
using LogViewer.Providers.API;
using System.Threading;
using System;
using YamlDotNet.Core.Tokens;
using System.Diagnostics;

namespace LogViewer.Controls.Helpers
{
    public class LogFetcher
    {
        public LogFetcher()
        {
        }

        public IProgress<double>? Progress { get; set; }

        public async Task<LogCollection> Load(ApiGatewayLogProvider provider, CancellationToken token)
        {
            var data = await provider.GetData(token, Progress);
            Progress?.Report(0);
            return data;
        }
    }
}
