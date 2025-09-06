using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HonyWing.UI.Converters
{
    /// <summary>
    /// 日志级别到颜色转换器
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-11 15:45:00
    /// @version: 1.0.0
    /// </summary>
    public class LogLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string level)
            {
                return level.ToUpperInvariant() switch
                {
                    "TRACE" => new SolidColorBrush(Color.FromRgb(128, 128, 128)), // 灰色
                    "DEBUG" => new SolidColorBrush(Color.FromRgb(0, 150, 255)),   // 蓝色
                    "INFO" => new SolidColorBrush(Color.FromRgb(0, 180, 0)),     // 绿色
                    "WARN" => new SolidColorBrush(Color.FromRgb(255, 165, 0)),   // 橙色
                    "ERROR" => new SolidColorBrush(Color.FromRgb(255, 69, 0)),   // 红色
                    "FATAL" => new SolidColorBrush(Color.FromRgb(139, 0, 0)),    // 深红色
                    _ => new SolidColorBrush(Color.FromRgb(0, 0, 0))             // 黑色（默认）
                };
            }

            return new SolidColorBrush(Color.FromRgb(0, 0, 0)); // 默认黑色
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}