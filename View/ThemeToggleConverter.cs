using System;
using System.Globalization;
using System.Windows.Data;

namespace VPNproject.View
{
    public class ThemeToggleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isDarkMode = (bool)value;
            return isDarkMode ? "Dark mode" : "Light mode";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}