namespace LogViewer.Providers.API
{
    public interface IApiDataProvider<T>
    {
        IAsyncEnumerable<T> GetData(CancellationToken token, IProgress<double>? progress = default);
    }
}
