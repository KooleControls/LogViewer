namespace LogViewer.Config.Hashing
{
    public interface IHashProvider
    {
        string ComputeHash(string input);
        Task<string> ComputeHashAsync(Stream stream, CancellationToken token);
        Task<string> CopyAndHashAsync(Stream source, Stream destination, CancellationToken token);
    }
}