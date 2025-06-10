namespace LogViewer.Logging
{
    public class LogEntryReader
    {
        private readonly LogEntry entry;

        public LogEntryReader(LogEntry entry)
        {
            this.entry = entry;
        }

        public bool TryGetByKey(LogKeys key, out string value)
        {
            return entry.Segments.TryGetValue(key, out value);
        }

        public bool TryGetByKey(LogKeys key, out DateTime value)
        {
            value = default;
            if (entry.Segments.TryGetValue(key, out string var))
            {
                return DateTime.TryParse(var, null, System.Globalization.DateTimeStyles.RoundtripKind, out value);
            }
            return false;
        }

        public bool TryGetByKey(LogKeys key, out float value)
        {
            value = default;
            if (entry.Segments.TryGetValue(key, out string var))
            {
                return float.TryParse(var, out value);
            }
            return false;
        }

        public bool TryGetByKey<T>(LogKeys key, out T value) where T : struct, Enum
        {
            value = default;
            if (entry.Segments.TryGetValue(key, out var stringValue))
            {
                return Enum.TryParse<T>(stringValue, ignoreCase: true, out value);
            }
            return false;
        }

    }


}









