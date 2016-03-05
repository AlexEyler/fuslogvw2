using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace NewFuslog
{
    public class Options
    {
        public static readonly Options Instance = new Options();

        private const string FuslogRegistrySubkeyName = @"SOFTWARE\Microsoft\Fusion";
        private const string EnableLogValueName = "EnableLog";
        private const string LogImmersiveValueName = "LogImmersive";
        private const string LogPathValueName = "LogPath";
        private const string LogFailuresValueName = "LogFailures";
        private const string LogSuccessesValueName = "ForceLog";

        private Options() { }

        public bool IsLogEnabled
        {
            get
            {
                bool? registryEntry = Utilities.RegistryEntryToNullableBool(Utilities.HKLMx64, FuslogRegistrySubkeyName, EnableLogValueName);
                return registryEntry.HasValue ? registryEntry.Value : false;
            }
        }

        public bool IsLoggingFailures
        {
            get
            {
                bool? registryEntry = Utilities.RegistryEntryToNullableBool(Utilities.HKLMx64, FuslogRegistrySubkeyName, LogFailuresValueName);
                return registryEntry.HasValue ? registryEntry.Value : false;
            }
        }

        public bool IsLogImmersive
        {
            get
            {
                bool? registryEntry = Utilities.RegistryEntryToNullableBool(Utilities.HKLMx64, FuslogRegistrySubkeyName, LogImmersiveValueName);
                return registryEntry.HasValue ? registryEntry.Value : false;
            }
        }

        public bool IsLoggingSuccesses
        {
            get
            {
                bool? registryEntry = Utilities.RegistryEntryToNullableBool(Utilities.HKLMx64, FuslogRegistrySubkeyName, LogSuccessesValueName);
                return registryEntry.HasValue ? registryEntry.Value : false;
            }
        }

        public string LogPath
        {
            get
            {
                return Utilities.RegistryEntryToString(Utilities.HKLMx64, FuslogRegistrySubkeyName, LogPathValueName);
            }
        }

        internal void Update(bool? isLogEnabled = null, bool? isLoggingFailures = null, bool? isLoggingSuccesses = null, bool? isLogImmersive = null, string logPath = null)
        {
            if (isLogEnabled.HasValue)
            {
                if (!Utilities.TrySetBoolRegistryEntry(Utilities.HKLMx64, FuslogRegistrySubkeyName, EnableLogValueName, isLogEnabled.Value))
                {
                    Debug.WriteLine("User does not have necessary priveleges.");
                }
            }

            if (isLoggingFailures.HasValue)
            {
                if (!Utilities.TrySetBoolRegistryEntry(Utilities.HKLMx64, FuslogRegistrySubkeyName, LogFailuresValueName, isLoggingFailures.Value))
                {
                    Debug.WriteLine("User does not have necessary priveleges.");
                }
            }

            if (isLoggingSuccesses.HasValue)
            {
                if (!Utilities.TrySetBoolRegistryEntry(Utilities.HKLMx64, FuslogRegistrySubkeyName, LogSuccessesValueName, isLoggingSuccesses.Value))
                {
                    Debug.WriteLine("User does not have necessary priveleges.");
                }
            }

            if (isLogImmersive.HasValue)
            {
                if (!Utilities.TrySetBoolRegistryEntry(Utilities.HKLMx64, FuslogRegistrySubkeyName, LogImmersiveValueName, isLogImmersive.Value))
                {
                    Debug.WriteLine("User does not have necessary priveleges.");
                }
            }

            if (string.IsNullOrEmpty(logPath))
            {
                using (RegistryKey key = Utilities.HKLMx64.OpenSubKey(FuslogRegistrySubkeyName, writable: true))
                {
                    if (key.GetValue(LogPathValueName) != null)
                    {
                        key.DeleteValue(LogPathValueName);
                    }
                }
            }
            else
            {
                if (!Directory.Exists(logPath))
                {
                    throw new DirectoryNotFoundException($"LogPath ({logPath}) must be created before being set");
                }

                if (!Utilities.TrySetStringRegistryEntry(Utilities.HKLMx64, FuslogRegistrySubkeyName, LogPathValueName, logPath))
                {
                    Debug.WriteLine("Don't have necessary priveleges.");
                }
            }
        }
    }
}
