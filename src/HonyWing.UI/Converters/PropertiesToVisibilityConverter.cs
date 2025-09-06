using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HonyWing.UI.Converters
{
    /// <summary>
    /// 属性集合到可见性转换器
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-13 16:00:00
    /// @version: 1.0.0
    /// </summary>
    public class PropertiesToVisibilityConverter : IValueConverter
    {
        public static PropertiesToVisibilityConverter Instance { get; } = new PropertiesToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection collection)
            {
                return collection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}