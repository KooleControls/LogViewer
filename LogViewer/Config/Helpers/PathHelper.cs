using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;

namespace LogViewer.Config.Helpers
{
    public static class PathHelper
    {
        public static bool IsPathRooted(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (IsRemotePath(path))
                return true; // URLs are always treated as "rooted"

            return Path.IsPathRooted(path);
        }

        public static string GetBasePath(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentNullException(nameof(source));

            if (IsRemotePath(source))
            {
                int lastSlash = source.LastIndexOf('/');
                if (lastSlash < 0)
                    throw new ArgumentException($"Invalid URL format: {source}");

                return source.Substring(0, lastSlash);
            }
            else
            {
                if (Path.IsPathRooted(source))
                {
                    return Path.GetDirectoryName(source) ?? throw new ArgumentException($"Cannot get directory name for {source}");
                }
                else
                {
                    throw new ArgumentException($"Source must be rooted or resolved first: {source}");
                }
            }
        }

        public static string CombinePaths(string basePath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
                throw new ArgumentNullException(nameof(basePath));
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentNullException(nameof(relativePath));

            if (IsRemotePath(basePath))
            {
                // Normalize slashes if needed
                if (!basePath.EndsWith("/"))
                    basePath += "/";

                // Avoid duplicate slashes
                if (relativePath.StartsWith("/"))
                    relativePath = relativePath.Substring(1);

                return basePath + relativePath;
            }
            else
            {
                return Path.Combine(basePath, relativePath);
            }
        }

        public static string NormalizePath(string path)
        {
            path = ReplaceWildcards(path);
            path = path.Replace("\\", "/");
            return path;
        }

        public static bool IsRemotePath(string path)
        {
            return path.StartsWith("http://") || path.StartsWith("https://");
        }

        private static string ReplaceWildcards(string url)
        {
            url = Environment.ExpandEnvironmentVariables(url);

            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            url = url.Replace("%APPVERSION%", $"v{version?.ToString()}");

            DirectoryInfo? slnPath = TryGetSolutionDirectoryInfo();
            url = url.Replace("%SOLUTION%", slnPath?.FullName);

            return url;
        }

        private static DirectoryInfo? TryGetSolutionDirectoryInfo(string currentPath = null)
        {
            var directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory;
        }
    }
}


