using LogViewer.Config.Helpers;
using System.Diagnostics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LogViewer.Favorites
{
    /// <summary>
    /// Reads and writes favorites.yaml next to the other per-user config
    /// (see <see cref="Config.ConfigurationService"/>). Saves on every change: the file is
    /// tiny and losing favorites to a crash would be annoying.
    ///
    /// Use <see cref="Shared"/> from both the WebAPI tab's star and the favorites list, so the
    /// two can't drift apart. It's a singleton rather than injected because there is exactly one
    /// favorites file per user; swap it for constructor injection if that stops being true.
    /// </summary>
    public class FavoritesStore
    {
        private static readonly string ConfigFolder = PathHelper.NormalizePath("%LOCALAPPDATA%\\LogViewer");
        private static readonly string FavoritesFile = PathHelper.CombinePaths(ConfigFolder, "favorites.yaml");

        private static FavoritesStore? _shared;

        /// <summary>The one every control talks to.</summary>
        public static FavoritesStore Shared => _shared ??= new FavoritesStore();

        private readonly List<Favorite> _favorites = new();

        /// <summary>Raised after any add or remove, so views can refresh.</summary>
        public event EventHandler? Changed;

        public IReadOnlyList<Favorite> Items => _favorites;

        public FavoritesStore()
        {
            Load();
        }

        public bool Contains(Favorite favorite)
            => _favorites.Any(f => f.IsSameGateway(favorite));

        public void Add(Favorite favorite)
        {
            if (Contains(favorite))
                return;

            _favorites.Add(favorite);
            Save();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Remove(Favorite favorite)
        {
            if (_favorites.RemoveAll(f => f.IsSameGateway(favorite)) == 0)
                return;

            Save();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Adds when missing, removes when present. Returns true if it is now a favorite.</summary>
        public bool Toggle(Favorite favorite)
        {
            if (Contains(favorite))
            {
                Remove(favorite);
                return false;
            }

            Add(favorite);
            return true;
        }

        public void Load()
        {
            _favorites.Clear();

            try
            {
                if (!File.Exists(FavoritesFile))
                    return;

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                var file = deserializer.Deserialize<FavoritesFileModel>(File.ReadAllText(FavoritesFile));
                if (file?.Favorites != null)
                    _favorites.AddRange(file.Favorites);
            }
            catch (Exception ex)
            {
                // A malformed favorites file shouldn't stop the app from starting.
                Debug.WriteLine($"Could not read {FavoritesFile}: {ex.Message}");
            }
        }

        private void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FavoritesFile)!);

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                File.WriteAllText(FavoritesFile,
                    serializer.Serialize(new FavoritesFileModel { Favorites = _favorites }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not write {FavoritesFile}: {ex.Message}");
                MessageBox.Show($"Could not save favorites:\r\n{ex.Message}", "Favorites",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private class FavoritesFileModel
        {
            public List<Favorite> Favorites { get; set; } = new();
        }
    }
}
