using LogViewer.AppContext;
using LogViewer.Files.Yml;

namespace LogViewer.Files
{
    /// <summary>The log of one house, named after the house.</summary>
    public record HouseLog(string Name, LogViewerContext Context);

    /// <summary>
    /// A folder with one .cyaml per house, so a fetched period can be analysed again (or opened
    /// in the main view via File > Open) without fetching it from the web API again.
    /// </summary>
    public static class HouseLogFolder
    {
        public static string RootFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LogViewer", "GapAnalysis");

        public static string CreateRunFolder(string? resortName)
        {
            string name = $"{ResortExportService.MakeSafeFileName(resortName)}_{DateTime.Now:yyyyMMdd_HHmm}";
            var folder = Path.Combine(RootFolder, name);
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static void Save(string folder, HouseLog house)
        {
            var file = Path.Combine(folder, ResortExportService.MakeSafeFileName(house.Name) + ".cyaml");
            using var stream = File.Create(file);
            new CyamlFileExporter().Export(stream, house.Context);
        }

        public static List<HouseLog> Load(string folder)
        {
            var houses = new List<HouseLog>();
            var importer = new CyamlFileImporter();
            foreach (var file in Directory.GetFiles(folder, "*.cyaml").Order(StringComparer.CurrentCultureIgnoreCase))
            {
                using var stream = File.OpenRead(file);
                if (importer.TryImport(stream, out var context))
                    houses.Add(new HouseLog(Path.GetFileNameWithoutExtension(file), context));
            }
            return houses;
        }
    }
}
