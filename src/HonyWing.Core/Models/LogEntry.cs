using System;
using System.ComponentModel;

namespace HonyWing.Core.Models
{
    /// <summary>
    /// 日志条目实体类
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-11 15:30:00
    /// @version: 1.0.0
    /// </summary>
    public class LogEntry : INotifyPropertyChanged
    {
        private DateTime _timestamp;
        private string _level = string.Empty;
        private string _logger = string.Empty;
        private string _message = string.Empty;
        private string? _exception;
        private string _formattedMessage = string.Empty;

        /// <summary>
        /// 日志时间戳
        /// </summary>
        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                if (_timestamp != value)
                {
                    _timestamp = value;
                    OnPropertyChanged(nameof(Timestamp));
                    OnPropertyChanged(nameof(TimeString));
                }
            }
        }

        /// <summary>
        /// 日志级别 (Trace, Debug, Info, Warn, Error, Fatal)
        /// </summary>
        public string Level
        {
            get => _level;
            set
            {
                if (_level != value)
                {
                    _level = value ?? string.Empty;
                    OnPropertyChanged(nameof(Level));
                }
            }
        }

        /// <summary>
        /// 日志记录器名称
        /// </summary>
        public string Logger
        {
            get => _logger;
            set
            {
                if (_logger != value)
                {
                    _logger = value ?? string.Empty;
                    OnPropertyChanged(nameof(Logger));
                }
            }
        }

        /// <summary>
        /// 日志消息内容
        /// </summary>
        public string Message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    _message = value ?? string.Empty;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }

        /// <summary>
        /// 异常信息（可选）
        /// </summary>
        public string? Exception
        {
            get => _exception;
            set
            {
                if (_exception != value)
                {
                    _exception = value;
                    OnPropertyChanged(nameof(Exception));
                    OnPropertyChanged(nameof(HasException));
                }
            }
        }

        /// <summary>
        /// 格式化后的完整日志消息
        /// </summary>
        public string FormattedMessage
        {
            get => _formattedMessage;
            set
            {
                if (_formattedMessage != value)
                {
                    _formattedMessage = value ?? string.Empty;
                    OnPropertyChanged(nameof(FormattedMessage));
                }
            }
        }

        /// <summary>
        /// 时间字符串格式
        /// </summary>
        public string TimeString => Timestamp.ToString("HH:mm:ss.fff");

        /// <summary>
        /// 是否包含异常信息
        /// </summary>
        public bool HasException => !string.IsNullOrEmpty(Exception);

        /// <summary>
        /// 日志级别的数值表示（用于排序和过滤）
        /// </summary>
        public int LevelValue => Level.ToUpperInvariant() switch
        {
            "TRACE" => 0,
            "DEBUG" => 1,
            "INFO" => 2,
            "WARN" => 3,
            "ERROR" => 4,
            "FATAL" => 5,
            _ => 2 // 默认为Info级别
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"[{TimeString}] [{Level}] {Logger} - {Message}";
        }
    }
}