using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using HonyWing.Core.Interfaces;
using HonyWing.Core.Models;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Targets;
using NLog.Common;
using Application = System.Windows.Application;

namespace HonyWing.Infrastructure.Services
{
    /// <summary>
    /// 自定义NLog目标，用于捕获日志事件
    /// </summary>
    public class ObservableTarget : TargetWithLayout
    {
        public event EventHandler<LogEventInfo>? LogReceived;

        protected override void Write(LogEventInfo logEvent)
        {
            LogReceived?.Invoke(this, logEvent);
        }
    }

    /// <summary>
    /// 日志服务实现类
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-11 15:40:00
    /// @version: 1.0.0
    /// </summary>
    public class LogService : ILogService
    {
        private readonly ILogger<LogService> _logger;
        private ObservableTarget? _observableTarget;
        private ICollectionView? _filteredView;
        private bool _disposed;

        public ObservableCollection<LogEntry> LogEntries { get; private set; }
        public int MaxLogEntries { get; set; } = 1000;
        public bool AutoScrollToLatest { get; set; } = true;
        
        private string _filterLevel = "All";
        public string FilterLevel
        {
            get => _filterLevel;
            set
            {
                if (_filterLevel != value)
                {
                    _filterLevel = value;
                    ApplyFilter();
                    FilterChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (_searchKeyword != value)
                {
                    _searchKeyword = value;
                    ApplyFilter();
                    FilterChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler<LogEntry>? LogEntryAdded;
        public event EventHandler? FilterChanged;

        public LogService(ILogger<LogService> logger)
        {
            _logger = logger;
            LogEntries = new ObservableCollection<LogEntry>();
        }

        public void Initialize()
        {
            try
            {
                // 创建并配置ObservableTarget
                _observableTarget = new ObservableTarget
                {
                    Name = "observableTarget",
                    Layout = "${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=tostring}"
                };

                // 订阅日志事件
                _observableTarget.LogReceived += OnLogEvent;

                // 添加到NLog配置
                var config = LogManager.Configuration ?? new NLog.Config.LoggingConfiguration();
                config.AddTarget(_observableTarget);
                config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, _observableTarget);
                LogManager.Configuration = config;

                _logger.LogInformation("日志服务初始化成功，ObservableTarget已配置");

                // 创建过滤视图
                _filteredView = CollectionViewSource.GetDefaultView(LogEntries);
                _filteredView.Filter = FilterLogEntry;

                _logger.LogInformation("日志服务初始化完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "日志服务初始化失败");
            }
        }

        private void OnLogEvent(object? sender, LogEventInfo e)
        {
            try
            {
                // 在UI线程中添加日志条目
                Application.Current?.Dispatcher.BeginInvoke(() =>
                {
                    var logEntry = new LogEntry
                    {
                        Timestamp = e.TimeStamp,
                        Level = e.Level.Name,
                        Logger = e.LoggerName ?? string.Empty,
                        Message = e.FormattedMessage ?? string.Empty,
                        Exception = e.Exception?.ToString(),
                        FormattedMessage = e.FormattedMessage ?? string.Empty
                    };

                    // 将最新日志插入到顶部
                    LogEntries.Insert(0, logEntry);
                    
                    // 限制日志条目数量，移除底部的旧日志
                    while (LogEntries.Count > MaxLogEntries)
                    {
                        LogEntries.RemoveAt(LogEntries.Count - 1);
                    }
                    LogEntryAdded?.Invoke(this, logEntry);
                });
            }
            catch (Exception ex)
            {
                // 避免在日志处理中再次记录日志导致循环
                System.Diagnostics.Debug.WriteLine($"处理日志事件时发生错误: {ex.Message}");
            }
        }

        public void ClearLogs()
        {
            try
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    LogEntries.Clear();
                });
                _logger.LogInformation("日志已清空");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空日志失败");
            }
        }

        public bool ExportLogs(string filePath, string format = "txt")
        {
            try
            {
                var logs = LogEntries.ToList();
                var content = new StringBuilder();

                if (format.ToLowerInvariant() == "csv")
                {
                    // CSV格式
                    content.AppendLine("Timestamp,Level,Logger,Message,Exception");
                    foreach (var log in logs)
                    {
                        content.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}\",\"{log.Level}\",\"{log.Logger}\",\"{log.Message.Replace("\"", "\"\"")}\",\"{log.Exception?.Replace("\"", "\"\"") ?? string.Empty}\"");
                    }
                }
                else
                {
                    // TXT格式
                    foreach (var log in logs)
                    {
                        content.AppendLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{log.Level}] {log.Logger} - {log.Message}");
                        if (!string.IsNullOrEmpty(log.Exception))
                        {
                            content.AppendLine($"Exception: {log.Exception}");
                        }
                        content.AppendLine();
                    }
                }

                File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
                _logger.LogInformation($"日志导出成功: {filePath} ({logs.Count} 条记录)");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"导出日志失败: {filePath}");
                return false;
            }
        }

        public void ApplyFilter()
        {
            try
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    _filteredView?.Refresh();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "应用日志过滤器失败");
            }
        }

        private bool FilterLogEntry(object item)
        {
            if (item is not LogEntry logEntry)
                return false;

            // 级别过滤
            if (FilterLevel != "All" && !string.Equals(logEntry.Level, FilterLevel, StringComparison.OrdinalIgnoreCase))
                return false;

            // 关键词搜索
            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                var keyword = SearchKeyword.ToLowerInvariant();
                return logEntry.Message.ToLowerInvariant().Contains(keyword) ||
                       logEntry.Logger.ToLowerInvariant().Contains(keyword) ||
                       (logEntry.Exception?.ToLowerInvariant().Contains(keyword) ?? false);
            }

            return true;
        }

        public int GetLogCount(string level)
        {
            return LogEntries.Count(log => string.Equals(log.Level, level, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    if (_observableTarget != null)
                    {
                        _observableTarget.LogReceived -= OnLogEvent;
                    }
                    
                    LogEntries.Clear();
                    _logger.LogInformation("日志服务资源已释放");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "释放日志服务资源时发生错误");
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }
}