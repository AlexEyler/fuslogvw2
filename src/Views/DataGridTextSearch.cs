using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NewFuslog
{
    public static class DataGridTextSearch
    {
        public static readonly DependencyProperty AppSearchValueProperty =
            DependencyProperty.RegisterAttached("AppSearchValue", typeof(string), typeof(DataGridTextSearch),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Inherits));


        public static readonly DependencyProperty DescriptionSearchValueProperty =
            DependencyProperty.RegisterAttached("DescriptionSearchValue", typeof(string), typeof(DataGridTextSearch),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty IsTextMatchProperty =
            DependencyProperty.RegisterAttached("IsTextMatch", typeof(bool), typeof(DataGridTextSearch),
                new UIPropertyMetadata(defaultValue: false));

        public static string GetAppSearchValue(this DependencyObject obj)
        {
            return (string)obj.GetValue(AppSearchValueProperty);
        }

        public static void SetAppSearchValue(this DependencyObject obj, string value)
        {
            obj.SetValue(AppSearchValueProperty, value);
        }

        public static string GetDescriptionSearchValue(this DependencyObject obj)
        {
            return (string)obj.GetValue(DescriptionSearchValueProperty);
        }

        public static void SetDescriptionSearchValue(this DependencyObject obj, string value)
        {
            obj.SetValue(DescriptionSearchValueProperty, value);
        }

        public static bool GetIsTextMatch(this DependencyObject obj)
        {
            return (bool)obj.GetValue(IsTextMatchProperty);
        }

        public static void SetIsTextMatch(this DependencyObject obj, bool value)
        {
            obj.SetValue(IsTextMatchProperty, value);
        }
    }

    public class SearchValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length % 2 != 0)
            {
                throw new ArgumentException($"{nameof(values)} must be an array of even length.");
            }

            bool allSearchesEmpty = true;
            bool allSearchesMatch = true;
            for (int i = 0; i <= values.Count() / 2; i += 2)
            {
                string cellText = values[i] == null ? string.Empty : values[i].ToString();
                string searchText = values[i + 1] as string;
                bool isEmpty = string.IsNullOrEmpty(searchText);

                allSearchesEmpty &= isEmpty;
                if (!isEmpty && !string.IsNullOrEmpty(cellText))
                {
                    allSearchesMatch &= cellText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1;
                }
            }

            return allSearchesEmpty || allSearchesMatch;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
