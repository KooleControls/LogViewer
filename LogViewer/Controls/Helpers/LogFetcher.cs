using LogViewer.Logging;
using LogViewer.Providers.API;

namespace LogViewer.Controls.Helpers
{
    public class LogFetcher
    {
        private CancellationTokenSource? cancellationTokenSource;

        public LogFetcher()
        {
        }

        public IProgress<double>? Progress { get; set; }
        public void Cancel() => cancellationTokenSource?.Cancel();

        public async Task<LogCollection> Load(ApiGatewayLogProvider provider)
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                return await provider.GetData(cancellationTokenSource.Token, Progress);
            }
            finally
            {
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                Progress?.Report(0);
            }
        }
    }
}
