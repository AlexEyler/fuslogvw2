using System;

namespace NewFuslog
{
    public class LogEntry
    {
        public string App
        {
            get;
        }

        public string LogPath
        {
            get;
        }

        public LogEntry(string app, string logPath)
        {
            this.App = app;
            this.LogPath = logPath;
        }

        public override bool Equals(object obj)
        {
            LogEntry other = obj as LogEntry;
            if (other != null)
            {
                return this.App.Equals(other.App, StringComparison.OrdinalIgnoreCase) && this.LogPath.Equals(other.LogPath, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int) (7 * this.App.ToLower().GetHashCode() + 53 * this.LogPath.ToLower().GetHashCode());
        }
    }
}