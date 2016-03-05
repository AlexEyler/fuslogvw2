using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NewFuslog
{
    public static class Utilities
    {
        internal static RegistryKey HKLMx64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

        public static bool IsElevated
        {
            get
            {
                return WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            }
        }

        internal static bool? RegistryEntryToNullableBool(RegistryKey baseKey, string subKeyName, string valueName)
        {
            using (RegistryKey key = baseKey.OpenSubKey(subKeyName))
            {
                object valueObj = key.GetValue(valueName);
                if (valueObj == null)
                {
                    return null;
                }

                return (int)valueObj == 0 ? false : true;
            }
        }

        internal static string RegistryEntryToString(RegistryKey baseKey, string subKeyName, string valueName)
        {
            using (RegistryKey key = baseKey.OpenSubKey(subKeyName))
            {
                object valueObj = key.GetValue(valueName);
                return valueObj?.ToString();
            }
        }

        internal static bool TrySetBoolRegistryEntry(RegistryKey baseKey, string subKeyName, string valueName, bool value)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(subKeyName, writable: true))
                {
                    key.SetValue(valueName, value ? 1 : 0, RegistryValueKind.DWord);
                    return true;
                }
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        internal static bool TrySetStringRegistryEntry(RegistryKey baseKey, string subKeyName, string valueName, string value)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(subKeyName, writable: true))
                {
                    key.SetValue(valueName, value, RegistryValueKind.String);
                    return true;
                }
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        internal static Color GetBackgroundColor(this Severity severity)
        {
            switch (severity)
            {
                case Severity.Error:
                    return Color.FromArgb(135, 255, 0, 0);
                case Severity.Warning:
                    return Color.FromArgb(135, 255, 255, 0);
                default:
                    return Color.FromArgb(0, 255, 255, 255);
            }
        }
    }
}
