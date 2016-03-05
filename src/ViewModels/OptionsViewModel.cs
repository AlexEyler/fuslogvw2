using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NewFuslog
{
    public class OptionsViewModel : BaseViewModel
    {
        private WindowViewModel parent;
        public List<string> Errors;

        public OptionsViewModel(WindowViewModel parent)
        {
            this.parent = parent;
            this.LogDirectory = Options.Instance.LogPath ?? string.Empty;
            this.EnableLog = Options.Instance.IsLogEnabled;
            this.LogFailures = Options.Instance.IsLoggingFailures;
            this.LogSuccesses = Options.Instance.IsLoggingSuccesses;
            this.LogImmersive = Options.Instance.IsLogImmersive;
            this.Errors = new List<string>();
        }

        private string logDirectory;
        public string LogDirectory
        {
            get
            {
                return this.logDirectory;
            }
            set
            {
                if (value != this.logDirectory)
                {
                    this.logDirectory = value;
                    this.NotifyPropertyChanged(nameof(this.LogDirectory));
                }
            }
        }

        private bool enableLog;
        public bool EnableLog
        {
            get
            {
                return this.enableLog;
            }
            set
            {
                if (value != this.enableLog)
                {
                    this.enableLog = value;
                    this.NotifyPropertyChanged(nameof(this.EnableLog));
                }
            }
        }

        private bool logFailures;
        public bool LogFailures
        {
            get
            {
                return this.logFailures;
            }
            set
            {
                if (value != this.logFailures)
                {
                    this.logFailures = value;
                    this.NotifyPropertyChanged(nameof(this.LogFailures));
                }
            }
        }

        private bool logSuccesses;
        public bool LogSuccesses
        {
            get
            {
                return this.logSuccesses;
            }
            set
            {
                if (value != this.logSuccesses)
                {
                    this.logSuccesses = value;
                    this.NotifyPropertyChanged(nameof(this.LogSuccesses));
                }
            }
        }

        private bool logImmersive;
        public bool LogImmersive
        {
            get
            {
                return this.logImmersive;
            }
            set
            {
                if (value != this.logImmersive)
                {
                    this.logImmersive = value;
                    this.NotifyPropertyChanged(nameof(this.LogImmersive));
                }
            }
        }

        private DelegateCommand browseForLogDirectoryCommand;
        public DelegateCommand BrowseForLogDirectoryCommand
        {
            get
            {
                if (this.browseForLogDirectoryCommand == null)
                {
                    this.browseForLogDirectoryCommand = new DelegateCommand(
                        this.BrowseForLogDirectory, this.CanBrowseForLogDirectory);
                }

                return this.browseForLogDirectoryCommand;
            }
        }

        private bool CanBrowseForLogDirectory(object unused) => true;

        private void BrowseForLogDirectory(object obj)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select the location you would like to drop the fuslog entries to.";
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;

            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.LogDirectory = dialog.SelectedPath;
            }
        }

        private DelegateCommand submitChangesCommand;
        public DelegateCommand SubmitChangesCommand
        {
            get
            {
                if (this.submitChangesCommand == null)
                {
                    this.submitChangesCommand = new DelegateCommand(
                        this.SubmitChanges, this.CanSubmitChanges);
                }

                return this.submitChangesCommand;
            }
        }

        private bool CanSubmitChanges(object unused) => true;

        private void SubmitChanges(object unused)
        {
            this.Errors.Clear();

            if (!Utilities.IsElevated)
            {
                this.Errors.Add($"You must run as administrator to make changes.");
            }

            if (!Path.IsPathRooted(this.LogDirectory))
            {
                this.Errors.Add($"Expected full path for log directory.");
            }

            if (this.Errors.Any())
            {
                this.ReportFailures();
                return;
            }

            if (!string.IsNullOrEmpty(this.LogDirectory) &&
                !Directory.Exists(this.LogDirectory))
            {
                var dialogResult = MessageBox.Show("The directory you've entered does not exist. Should we try to create it for you?",
                    "Directory does not exist", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(this.LogDirectory);
                    }
                    catch (Exception e)
                    {
                        this.Errors.Add($"Could not create directory at specified location: {e.Message}");
                        this.ReportFailures();
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            Options.Instance.Update(
                isLogEnabled: this.EnableLog,
                isLoggingFailures: this.LogFailures,
                isLoggingSuccesses: this.LogSuccesses,
                isLogImmersive: this.LogImmersive,
                logPath: this.LogDirectory);

            // Report a "no" error list to clear out old entries
            this.ReportFailures();
        }

        private void ReportFailures()
        {
            var asMessages = from e in this.Errors
                                       let severity = Severity.Error
                                       select new MessageViewModel(e, severity);

            parent.SendCurrentMessageGroup(this, asMessages);
        }
    }
}
