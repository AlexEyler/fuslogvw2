using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewFuslog
{
    public class LogEntryViewModel : BaseViewModel
    {
        private readonly LogEntry logEntry;
        public LogEntryViewModel(LogEntry logEntry)
        {
            this.logEntry = logEntry;
            this.LogPath = logEntry.LogPath;
            this.App = logEntry.App;
            this.Description = Path.GetFileNameWithoutExtension(logEntry.LogPath);
            this.WriteTime = File.GetLastWriteTime(logEntry.LogPath);
        }

        internal string LogPath { get; }

        private string app;
        public string App
        {
            get
            {
                return app;
            }
            set
            {
                if (value != app)
                {
                    this.NotifyPropertyChanged(nameof(App));
                    app = value;
                }
            }
        }

        private string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                if (value != description)
                {
                    this.NotifyPropertyChanged(nameof(Description));
                    description = value;
                }
            }
        }

        private DateTime writeTime;
        public DateTime WriteTime
        {
            get
            {
                return writeTime;
            }
            set
            {
                if (value != writeTime)
                {
                    this.NotifyPropertyChanged(nameof(WriteTime));
                    writeTime = value;
                }
            }
        }

        public bool Represents(LogEntry entry)
        {
            if (entry == null)
            {
                return false;
            }

            return this.logEntry.Equals(entry);
        }

    }
}
