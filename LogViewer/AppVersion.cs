namespace LogViewer
{
    public readonly struct AppVersion
    {
        public Version BaseVersion { get; }     // MAJOR.MINOR.PATCH
        public bool IsPrerelease { get; }
        public string PrereleaseTag { get; }    // "b3" or ""

        public AppVersion(Version baseVersion, string prereleaseTag = "")
        {
            prereleaseTag ??= string.Empty;

            BaseVersion = baseVersion;
            PrereleaseTag = prereleaseTag;
            IsPrerelease = prereleaseTag.Length > 0;
        }

        public override string ToString()
        {
            var core = $"v{BaseVersion.Major}.{BaseVersion.Minor}.{BaseVersion.Build}";
            return IsPrerelease ? $"{core}-{PrereleaseTag}" : core;
        }

        public static bool TryParse(string versionString, out AppVersion appVersion)
        {
            appVersion = default;

            // String sanitization
            versionString = versionString.TrimStart('v');
            if (string.IsNullOrWhiteSpace(versionString))
                return false;

            // Split into base and prerelease/build parts
            var parts = versionString.Split('-', '+');

            if (!Version.TryParse(parts[0], out var baseVersion))
                return false;

            var prereleasePart = parts.Length > 1 ? parts[1] : string.Empty;
            appVersion = new AppVersion(baseVersion, prereleasePart);
            return true;
        }
    }
}




