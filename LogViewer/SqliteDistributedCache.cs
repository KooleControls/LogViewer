using LogViewer.Config.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Distributed;
using System.Reflection;

namespace LogViewer
{
    public class SqliteDistributedCache : IDistributedCache
    {
        private readonly string _connectionString;

        public SqliteDistributedCache(string dbPath)
        {
            var dir = Path.GetDirectoryName(dbPath)!;
            Directory.CreateDirectory(dir);
            _connectionString = $"Data Source={dbPath}";
            EnsureSchema();
        }

        private void EnsureSchema()
        {

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Cache (
                Key TEXT PRIMARY KEY,
                Value BLOB,
                Expiration DATETIME
            );";
            cmd.ExecuteNonQuery();
        }

        public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();

        public void Refresh(string key) => RefreshAsync(key).GetAwaiter().GetResult();

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => SetAsync(key, value, options).GetAwaiter().GetResult();

        public byte[]? Get(string key) => GetAsync(key).GetAwaiter().GetResult();

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(token);
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT Value FROM Cache 
            WHERE Key = @key AND (Expiration IS NULL OR Expiration > CURRENT_TIMESTAMP)";
            cmd.Parameters.AddWithValue("@key", key);
            var result = await cmd.ExecuteScalarAsync(token);
            return result as byte[];
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            DateTime? expiration = null;
            if (options.AbsoluteExpiration.HasValue)
                expiration = options.AbsoluteExpiration.Value.UtcDateTime;
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
                expiration = DateTime.UtcNow + options.AbsoluteExpirationRelativeToNow.Value;

            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(token);
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO Cache (Key, Value, Expiration)
            VALUES (@key, @value, @expiration)
            ON CONFLICT(Key) DO UPDATE SET
                Value = excluded.Value,
                Expiration = excluded.Expiration;";
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@value", value);
            cmd.Parameters.AddWithValue("@expiration", (object?)expiration ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(token);
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(token);
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Cache WHERE Key = @key";
            cmd.Parameters.AddWithValue("@key", key);
            await cmd.ExecuteNonQueryAsync(token);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            // No-op unless you want to update sliding expiration timestamps
            return Task.CompletedTask;
        }
    }

}
