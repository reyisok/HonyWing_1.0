using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HonyWing.UI.Converters
{
    /// <summary>
    /// 字符串到可见性转换器
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-13 16:00:00
    /// @version: 1.0.0
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public static StringToVisibilityConverter Instance { get; } = new StringToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}