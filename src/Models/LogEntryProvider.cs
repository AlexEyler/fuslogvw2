using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NewFuslog
{
    public class LogEntryProvider : IDisposable
    {
        public const string DefaultLogDirectoryName = "Default";
        public const string NativeImageLogDirectoryName = "NativeImage";

        private readonly Action<string, string> addEntry;
        private readonly Action<string, string> updateEntry;
        private readonly Action<string, string> removeEntry;
        private string logDirectory;
        private readonly object logEntryFileSystemWatchersLock = new object();
        private ConcurrentDictionary<string, FileSystemWatcher> logEntryFileSystemWatchers;
        private IEnumerable<FileSystemWatcher> appFileSystemWatchers;


        public LogEntryProvider(Action<string, string> addEntry, Action<string, string> updateEntry, Action<string, string> removeEntry)
        {
            this.addEntry = addEntry;
            this.updateEntry = updateEntry;
            this.removeEntry = removeEntry;
        }

        public void Initialize()
        {
            this.logDirectory = Options.Instance.LogPath;
            if (string.IsNullOrEmpty(logDirectory))
            {
                // we're going to skip for now -- this needs to somehow find the files from the INetCache
                return;
            }

            // Initialize app name file watchers
            this.appFileSystemWatchers = new FileSystemWatcher[]
            {
                new FileSystemWatcher(Path.Combine(this.logDirectory, DefaultLogDirectoryName)),
                new FileSystemWatcher(Path.Combine(this.logDirectory, NativeImageLogDirectoryName))
            };

            foreach (var watcher in this.appFileSystemWatchers)
            {
                watcher.Changed += AppChanged;
                watcher.Created += AppChanged;
                watcher.Deleted += AppRemoved;
                watcher.EnableRaisingEvents = true;
            }

            // Initialize log entry file watchers
            this.logEntryFileSystemWatchers = new ConcurrentDictionary<string, FileSystemWatcher>();
            foreach (var imageTypeDir in Directory.EnumerateDirectories(this.logDirectory))
            {
                foreach (var appDir in Directory.EnumerateDirectories(imageTypeDir))
                {
                    this.AddWatcherToLogEntries(appDir);
                }
            }
        }

        private void AppRemoved(object sender, FileSystemEventArgs e)
        {
            this.RemoveWatcherFromLogEntries(e.FullPath);
        }

        private void AppChanged(object sender, FileSystemEventArgs e)
        {
            this.AddWatcherToLogEntries(e.FullPath);
        }

        private void FileRemoved(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                string appName = new DirectoryInfo(e.FullPath).Parent.Name;
                this.removeEntry(appName, e.FullPath);
            }
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(e.FullPath);
            string appName = dir.Parent.Name;

            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                this.updateEntry(appName, e.FullPath);
            }
            else if (e.ChangeType == WatcherChangeTypes.Created)
            {
                this.addEntry(appName, e.FullPath);
            }
        }

        private void AddWatcherToLogEntries(string fullPath)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(fullPath);
            watcher.Changed += FileChanged;
            watcher.Created += FileChanged;
            watcher.Deleted += FileRemoved;
            watcher.EnableRaisingEvents = true;

            this.logEntryFileSystemWatchers.TryAdd(Path.GetFileName(fullPath), watcher);
        }

        private void RemoveWatcherFromLogEntries(string fullPath)
        {
            string appName = Path.GetFileName(fullPath);
            FileSystemWatcher watcher = null;
            var watcherKvp = this.logEntryFileSystemWatchers.FirstOrDefault(w => w.Value.Path.Equals(fullPath, StringComparison.OrdinalIgnoreCase));

            if (watcherKvp.Value != null)
            {
                watcher = watcherKvp.Value;
            }

            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= FileChanged;
                watcher.Created -= FileChanged;
                watcher.Deleted -= FileRemoved;
                watcher.Dispose();

                this.logEntryFileSystemWatchers.TryRemove(appName, out watcher);
            }
        }


        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                if (this.logEntryFileSystemWatchers != null)
                {
                    foreach (var key in this.logEntryFileSystemWatchers.Keys)
                    {
                        FileSystemWatcher watcher;
                        if (this.logEntryFileSystemWatchers.TryRemove(key, out watcher) && watcher != null)
                        {
                            watcher.EnableRaisingEvents = false;
                            watcher.Changed -= FileChanged;
                            watcher.Created -= FileChanged;
                            watcher.Deleted -= FileRemoved;
                            watcher.Dispose();
                        }
                    }

                    this.logEntryFileSystemWatchers = null;

                    foreach (var watcher in this.appFileSystemWatchers)
                    {
                        watcher.EnableRaisingEvents = false;
                        watcher.Changed -= AppChanged;
                        watcher.Created -= AppChanged;
                        watcher.Deleted -= AppRemoved;
                        watcher.Dispose();
                    }

                    this.appFileSystemWatchers = null;
                }

                this.disposed = true;
            }
        }
    }
}
