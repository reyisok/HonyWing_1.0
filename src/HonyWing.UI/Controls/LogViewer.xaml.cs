using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HonyWing.Core.Models;
using HonyWing.UI.ViewModels;

namespace HonyWing.UI.Controls
{
    /// <summary>
    /// LogViewer.xaml 的交互逻辑
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-11 15:50:00
    /// @version: 1.0.0
    /// </summary>
    public partial class LogViewer : UserControl
    {
        private bool _isScrollEnabled = true;
        private readonly List<LogEntry> _pausedLogs = new List<LogEntry>();

        public LogViewer()
        {
            InitializeComponent();

            // 设置ComboBox的默认选中项
            LevelFilterComboBox.SelectedValue = "All"; // 默认选择"All"
        }

        private void LogViewer_Loaded(object sender, RoutedEventArgs e)
        {
            // 如果启用了自动滚动，滚动到最新日志
            if (DataContext is ILogViewerViewModel viewModel && viewModel.AutoScrollToLatest)
            {
                ScrollToLatestLog();
            }
        }

        private void LogDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // 如果启用了自动滚动，滚动到顶部显示最新日志
            if (DataContext is ILogViewerViewModel viewModel && viewModel.AutoScrollToLatest)
            {
                // 延迟滚动到顶部，确保新行已完全加载
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (LogDataGrid.Items.Count > 0)
                    {
                        LogDataGrid.ScrollIntoView(LogDataGrid.Items[0]);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void ScrollToLatestLog()
        {
            if (LogDataGrid.Items.Count > 0)
            {
                // 最新日志现在在顶部（索引0），滚动到第一项
                var firstItem = LogDataGrid.Items[0];
                LogDataGrid.ScrollIntoView(firstItem);
            }
        }

        // 双击行显示详细信息
        private void LogDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LogDataGrid.SelectedItem is LogEntry logEntry)
            {
                ShowLogDetails(logEntry);
            }
        }

        private void ShowLogDetails(LogEntry logEntry)
        {
            var detailWindow = new Window
            {
                Title = $"日志详情 - {logEntry.Level}",
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            var textBox = new TextBox
            {
                Text = $"时间: {logEntry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}\n" +
                       $"级别: {logEntry.Level}\n" +
                       $"记录器: {logEntry.Logger}\n" +
                       $"消息: {logEntry.Message}\n" +
                       (string.IsNullOrEmpty(logEntry.Exception) ? "" : $"\n异常信息:\n{logEntry.Exception}"),
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas, Courier New"),
                FontSize = 12,
                Margin = new Thickness(10)
            };

            detailWindow.Content = textBox;
            detailWindow.ShowDialog();
        }

        /// <summary>
        /// 搜索框回车键事件处理
        /// </summary>
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is LogViewerViewModel viewModel)
                {
                    // 当无输入值时，默认不执行筛选
                    if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
                    {
                        viewModel.SearchCommand?.Execute(null);
                    }
                    else
                    {
                        // 清空搜索关键字并刷新
                        viewModel.SearchKeyword = string.Empty;
                        viewModel.SearchCommand?.Execute(null);
                    }
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// 滚动切换按钮点击事件
        /// </summary>
        private void ScrollToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isScrollEnabled = !_isScrollEnabled;

            if (_isScrollEnabled)
            {
                // 恢复滚动
                ScrollToggleButton.Content = "停止滚动";

                // 恢复停止期间的日志并继续滚动
                if (DataContext is LogViewerViewModel viewModel)
                {
                    // 这里可以添加恢复暂停期间日志的逻辑
                    // 目前简单地重新启用自动滚动
                    viewModel.AutoScrollToLatest = true;
                }
            }
            else
            {
                // 停止滚动
                ScrollToggleButton.Content = "滚动日志";

                if (DataContext is LogViewerViewModel viewModel)
                {
                    viewModel.AutoScrollToLatest = false;
                }
            }
        }
    }

    /// <summary>
    /// 日志查看器ViewModel接口
    /// </summary>
    public interface ILogViewerViewModel
    {
        bool AutoScrollToLatest { get; }
        System.Collections.ObjectModel.ObservableCollection<HonyWing.Core.Models.LogEntry> LogEntries { get; }
        System.Windows.Input.ICommand ClearLogsCommand { get; }
        System.Windows.Input.ICommand ExportLogsCommand { get; }
    }
}
