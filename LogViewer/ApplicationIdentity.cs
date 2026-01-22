using System.Reflection;

namespace LogViewer
{

    public static class ApplicationIdentity
    {
        public static AppVersion GetAppVersion()
        {
            // Prefer InformationalVersion (e.g. "3.9.0-b3" or "3.9.0")
            var asm = Assembly.GetExecutingAssembly();
            var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            if (!string.IsNullOrWhiteSpace(info) && AppVersion.TryParse(info, out var parsed))
                return parsed;

            // Fallback: numeric assembly version -> base only
            var v = asm.GetName().Version;
            if (v != null)
            {
                var baseVersion = new Version(v.Major, v.Minor, v.Build < 0 ? 0 : v.Build);
                return new AppVersion(baseVersion, "");
            }

            return new AppVersion(new Version(0, 0, 0), "");
        }
    }
}




