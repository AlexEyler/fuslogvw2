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
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            bool not = false;
            if (parameter != null && parameter.ToString().Equals("!", StringComparison.InvariantCulture))
            {
                not = true;
            }

            if (not)
            {
                return boolValue ? Visibility.Hidden : Visibility.Visible;
            }

            return boolValue ? Visibility.Visible : Visibility.Hidden;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibleValue = (Visibility)value;
            return visibleValue == Visibility.Visible ? true : false;
        }
    }
}
