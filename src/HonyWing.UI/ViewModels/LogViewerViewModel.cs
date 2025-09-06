using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using HonyWing.Core.Interfaces;
using HonyWing.Core.Models;
using HonyWing.UI.Commands;
using HonyWing.UI.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace HonyWing.UI.ViewModels
{
    /// <summary>
    /// 日志查看器ViewModel
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-11 15:55:00
    /// @version: 1.0.0
    /// </summary>
    public class LogViewerViewModel : INotifyPropertyChanged, ILogViewerViewModel
    {
        private readonly ILogService _logService;
        private readonly ILogger<LogViewerViewModel> _logger;
        private ICollectionView? _filteredView;

        public ObservableCollection<LogEntry> LogEntries => _logService.LogEntries;

        /// <summary>
        /// 过滤后的日志条目（用于UI绑定）
        /// </summary>
        public ICollectionView FilteredLogs => _filteredView ?? CollectionViewSource.GetDefaultView(_logService.LogEntries);

        /// <summary>
        /// 总日志数量
        /// </summary>
        public int TotalLogCount => _logService.LogEntries.Count;

        /// <summary>
        /// 过滤后的日志数量
        /// </summary>
        public int FilteredLogCount => _logService.LogEntries.Count(log => FilterLogEntry((object)log));

        public string FilterLevel
        {
            get => _logService.FilterLevel;
            set
            {
                if (_logService.FilterLevel != value)
                {
                    _logService.FilterLevel = value;
                    OnPropertyChanged();
                    // Level筛选立即生效
                    RefreshFilter();
                }
            }
        }

        public string SearchKeyword
        {
            get => _logService.SearchKeyword;
            set
            {
                if (_logService.SearchKeyword != value)
                {
                    _logService.SearchKeyword = value;
                    OnPropertyChanged();
                    // 只有在用户主动执行搜索时才刷新过滤视图
                    // 这里不自动刷新，等待用户按回车键或点击搜索按钮
                }
            }
        }

        public bool AutoScrollToLatest
        {
            get => _logService.AutoScrollToLatest;
            set
            {
                if (_logService.AutoScrollToLatest != value)
                {
                    _logService.AutoScrollToLatest = value;
                    OnPropertyChanged();
                }
            }
        }

        public int MaxLogEntries
        {
            get => _logService.MaxLogEntries;
            set
            {
                if (_logService.MaxLogEntries != value)
                {
                    _logService.MaxLogEntries = value;
                    OnPropertyChanged();
                }
            }
        }

        // 统计属性
        public int ErrorCount => _logService.GetLogCount("Error") + _logService.GetLogCount("Fatal");
        public int WarningCount => _logService.GetLogCount("Warn");
        public int InfoCount => _logService.GetLogCount("Info");
        public int DebugCount => _logService.GetLogCount("Debug");
        public int TraceCount => _logService.GetLogCount("Trace");

        // 命令
        public ICommand ClearLogsCommand { get; }
        public ICommand ExportLogsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }

        public LogViewerViewModel(ILogService logService, ILogger<LogViewerViewModel> logger)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // 初始化过滤视图
            _filteredView = CollectionViewSource.GetDefaultView(_logService.LogEntries);
            _filteredView.Filter = FilterLogEntry;

            // 初始化命令
            ClearLogsCommand = new RelayCommand(ClearLogs);
            ExportLogsCommand = new RelayCommand(ExportLogs);
            RefreshCommand = new RelayCommand(Refresh);
            SearchCommand = new RelayCommand(ExecuteSearch);

            // 订阅日志服务事件
            _logService.LogEntryAdded += OnLogEntryAdded;
            _logService.FilterChanged += OnFilterChanged;
        }

        private void OnLogEntryAdded(object? sender, LogEntry logEntry)
        {
            // 更新统计信息
            OnPropertyChanged(nameof(ErrorCount));
            OnPropertyChanged(nameof(WarningCount));
            OnPropertyChanged(nameof(InfoCount));
            OnPropertyChanged(nameof(DebugCount));
            OnPropertyChanged(nameof(TraceCount));
            OnPropertyChanged(nameof(TotalLogCount));
            OnPropertyChanged(nameof(FilteredLogCount));
        }

        private void OnFilterChanged(object? sender, EventArgs e)
        {
            // 过滤变更时更新UI
            OnPropertyChanged(nameof(FilterLevel));
            OnPropertyChanged(nameof(SearchKeyword));
            OnPropertyChanged(nameof(FilteredLogCount));
        }

        public void ClearLogs()
        {
            try
            {
                _logService.ClearLogs();
                _logger.LogInformation("用户清空日志");

                // 更新统计信息
                OnPropertyChanged(nameof(ErrorCount));
                OnPropertyChanged(nameof(WarningCount));
                OnPropertyChanged(nameof(InfoCount));
                OnPropertyChanged(nameof(DebugCount));
                OnPropertyChanged(nameof(TraceCount));
                OnPropertyChanged(nameof(TotalLogCount));
                OnPropertyChanged(nameof(FilteredLogCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空日志失败");
            }
        }

        public void ExportLogs()
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv",
                    DefaultExt = "txt",
                    FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var format = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLowerInvariant() == ".csv" ? "csv" : "txt";
                    var success = _logService.ExportLogs(saveFileDialog.FileName, format);

                    if (success)
                    {
                        _logger.LogInformation("日志导出成功: {FilePath}", saveFileDialog.FileName);
                        _logger.LogInformation("日志导出成功: {FilePath}", saveFileDialog.FileName);
                    }
                    else
                    {
                        _logger.LogWarning("日志导出失败: {FilePath}", saveFileDialog.FileName);
                        _logger.LogWarning("日志导出失败，请检查文件路径和权限");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导出日志时发生异常");
                _logger.LogError(ex, "日志导出过程中发生错误");
            }
        }

        private void Refresh()
        {
            try
            {
                RefreshFilter();
                _logger.LogDebug("日志视图已刷新");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新日志视图失败");
            }
        }

        /// <summary>
        /// 执行搜索命令（回车键触发）
        /// </summary>
        private void ExecuteSearch()
        {
            try
            {
                RefreshFilter();
                _logger.LogDebug("执行日志搜索，关键字: {SearchKeyword}", SearchKeyword ?? "<空>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行日志搜索失败");
            }
        }

        /// <summary>
        /// 刷新过滤视图
        /// </summary>
        private void RefreshFilter()
        {
            try
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    _filteredView?.Refresh();
                    OnPropertyChanged(nameof(FilteredLogCount));
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新过滤视图失败");
            }
        }

        /// <summary>
        /// 过滤日志条目
        /// </summary>
        /// <param name="item">日志条目对象</param>
        /// <returns>是否通过过滤</returns>
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            // 取消订阅事件
            if (_logService != null)
            {
                _logService.LogEntryAdded -= OnLogEntryAdded;
                _logService.FilterChanged -= OnFilterChanged;
            }
        }
    }
}
