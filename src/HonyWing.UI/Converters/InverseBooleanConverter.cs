using System;
using System.Globalization;
using System.Windows.Data;

namespace HonyWing.UI.Converters
{
    /// <summary>
    /// 反向布尔转换器
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-13 15:45:00
    /// @version: 1.0.0
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }
}