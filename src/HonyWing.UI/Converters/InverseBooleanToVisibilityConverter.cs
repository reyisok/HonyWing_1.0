using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HonyWing.UI.Converters
{
    /// <summary>
    /// 反向布尔到可见性转换器
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-11 16:30:00
    /// @version: 1.0.0
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Collapsed;
            }
            return false;
        }
    }
}