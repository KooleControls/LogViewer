
using System.Reflection;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace LogViewer.Logging
{
    public class LogEntry
    {
        public Dictionary<LogKeys, string> Segments { get; private set; } = new();
    }
}









