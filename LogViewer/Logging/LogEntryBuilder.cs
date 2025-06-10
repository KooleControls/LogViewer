
using System.Globalization;
using System.Text;

namespace LogViewer.Logging
{
    public class LogEntryBuilder
    {
        private LogEntry entry = new LogEntry();

        public LogEntry Build()
        {
            return entry;
        }

        public static LogEntryBuilder CreateBuilder(DateTime timeStamp)
        {
            LogEntryBuilder builder = new LogEntryBuilder();
            builder.WithSegment(LogKeys.TimeStamp, timeStamp);
            return builder;
        }

        public LogEntryBuilder WithSegment(LogKeys key, DateTime value)
        {
            entry.Segments.Add(key, value.ToString("o"));
            return this;
        }

        public LogEntryBuilder WithSegment(LogKeys key, float value)
        {
            entry.Segments.Add(key, value.ToString());
            return this;
        }
        public LogEntryBuilder WithSegment(LogKeys key, int value)
        {
            entry.Segments.Add(key, value.ToString());
            return this;
        }

        public LogEntryBuilder WithSegment(LogKeys key, string value)
        {
            entry.Segments.Add(key, value);
            return this;
        }

        public LogEntryBuilder WithSegment<T>(LogKeys key, T value) where T : struct, Enum
        {
            entry.Segments.Add(key, value.ToString());
            return this;
        }

        public LogEntryBuilder WithSegment(LogKeys key, byte[] value)
        {
            entry.Segments.Add(key, BitConverter.ToString(value).Replace("-", ""));
            return this;
        }
    }
}









