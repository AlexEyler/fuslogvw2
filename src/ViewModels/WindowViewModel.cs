using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace NewFuslog
{
    public class WindowViewModel : BaseViewModel
    {
        private readonly object messagesLock = new object();
        private readonly object logEntriesLock = new object();
        private Dictionary<BaseViewModel, IEnumerable<MessageViewModel>> currentMessages;
        private Dispatcher mainThread;

        public WindowViewModel()
        {
            this.mainThread = Dispatcher.CurrentDispatcher;
            this.LogEntries = new ConcurrentObservableCollection<LogEntryViewModel>();
            BindingOperations.EnableCollectionSynchronization(this.LogEntries, this.logEntriesLock);
            this.LogEntryProvider = new LogEntryProvider(
                addEntry: (appName, fullPath) =>
                {
                    // Addition to LogEntries is thread-safe (synchronized) because we enabled it in
                    // BindingOperations.EnableCollectionSynchronization
                    LogEntry logEntry = new LogEntry(appName, fullPath);

                    int logEntriesCount = 0;
                    lock (this.logEntriesLock)
                    {
                        this.LogEntries.Add(new LogEntryViewModel(logEntry));
                        logEntriesCount = this.LogEntries.Count;
                    }

                    if (logEntriesCount == 1)
                    {
                        this.mainThread.Invoke(() => this.ClearEntriesCommand.RaiseCanExecuteChanged());
                    }
                },
                updateEntry: (appName, fullPath) =>
                {
                    LogEntry logEntry;
                    LogEntryViewModel viewModel;
                    lock (this.logEntriesLock)
                    {
                        logEntry = new LogEntry(appName, fullPath);
                        viewModel = this.LogEntries.FirstOrDefault(vm => vm.Represents(logEntry));
                    }

                    if (viewModel != null)
                    {
                        viewModel.WriteTime = File.GetLastWriteTime(viewModel.LogPath);
                    }
                    else
                    {
                        int logEntriesCount = 0;
                        lock (this.logEntriesLock)
                        {
                            this.LogEntries.Add(new LogEntryViewModel(logEntry));
                            logEntriesCount = this.LogEntries.Count;
                        }

                        if (logEntriesCount == 1)
                        {
                            this.mainThread.Invoke(() => this.ClearEntriesCommand.RaiseCanExecuteChanged());
                        }
                    }
                },
                removeEntry: (appName, fullPath) =>
                {
                    // don't do anything
                });

            this.Options = new OptionsViewModel(this);
            this.Messages = new ObservableCollection<MessageViewModel>();
            this.currentMessages = new Dictionary<BaseViewModel, IEnumerable<MessageViewModel>>();
            this.AddWarningIfNotElevated();

            this.LogEntryProvider.Initialize();
        }

        public ObservableCollection<LogEntryViewModel> LogEntries { get; private set; }
        public ObservableCollection<MessageViewModel> Messages { get; private set; }
        public OptionsViewModel Options { get; private set; }
        public LogEntryProvider LogEntryProvider { get; private set; }

        private DelegateCommand clearMessagesCommand;
        public DelegateCommand ClearMessagesCommand
        {
            get
            {
                if (clearMessagesCommand == null)
                {
                    clearMessagesCommand = new DelegateCommand(ClearMessages, CanClearMessages);
                }

                return clearMessagesCommand;
            }
        }

        private bool CanClearMessages(object unused)
        {
            return this.Messages.Count > 0;
        }

        private void ClearMessages(object obj)
        {
            this.currentMessages.Clear();
            this.UpdateMessages();
        }

        private DelegateCommand openLogEntryCommand;
        public DelegateCommand OpenLogEntryCommand
        {
            get
            {
                if (this.openLogEntryCommand == null)
                {
                    this.openLogEntryCommand = new DelegateCommand(
                        this.OpenLogEntry, this.CanOpenLogEntry);
                }

                return this.openLogEntryCommand;
            }
        }

        private bool CanOpenLogEntry(object unused) => true;

        private void OpenLogEntry(object selectedValue)
        {
            LogEntryViewModel entry = selectedValue as LogEntryViewModel;
            if (entry != null)
            {
                if (File.Exists(entry.LogPath))
                {
                    Process.Start(entry.LogPath);
                }
            }
        }

        private DelegateCommand clearEntriesCommand;
        public DelegateCommand ClearEntriesCommand
        {
            get
            {
                if (this.clearEntriesCommand == null)
                {
                    this.clearEntriesCommand = new DelegateCommand(
                        ClearEntries, CanClearEntries);
                }

                return this.clearEntriesCommand;
            }
        }

        private bool CanClearEntries(object obj)
        {
            lock (this.logEntriesLock)
            {
                return this.LogEntries.Count > 0;
            }
        }

        private void ClearEntries(object obj)
        {
            lock (this.logEntriesLock)
            {
                this.LogEntries.Clear();
                this.mainThread.Invoke(() => this.ClearEntriesCommand.RaiseCanExecuteChanged());
            }
        }

        internal void SendCurrentMessageGroup(BaseViewModel sender, IEnumerable<MessageViewModel> messages)
        {
            this.currentMessages[sender] = messages;
            this.UpdateMessages();
        }

        private void UpdateMessages()
        {
            lock (this.messagesLock)
            {
                this.Messages.Clear();

                foreach (var key in this.currentMessages.Keys)
                {
                    foreach (var message in this.currentMessages[key])
                    {
                        this.Messages.Add(message);
                    }
                }
            }

            this.mainThread.Invoke(() => this.ClearMessagesCommand.RaiseCanExecuteChanged());
        }

        private void AddWarningIfNotElevated()
        {
            if (!Utilities.IsElevated)
            {
                this.currentMessages[this] = new MessageViewModel[]
                {
                    new MessageViewModel("If you would like to change any options, you must be run this program as administrator.", Severity.Warning)
                };

                this.UpdateMessages();
            }
        }
    }
}
