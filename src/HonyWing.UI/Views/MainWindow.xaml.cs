using HonyWing.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using NLog;

namespace HonyWing.UI.Views;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    // 移除未使用的字段

    public MainWindow()
    {
        InitializeComponent();

        // 设置数据上下文
        DataContext = App.ServiceProvider?.GetService<MainWindowViewModel>();

        // 订阅窗口事件
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;

        // 订阅应用程序退出事件已移除（字段未使用）

        // 注册快捷键
        RegisterKeyboardShortcuts();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 窗口加载完成后的初始化逻辑
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Initialize();
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        var logger = NLog.LogManager.GetCurrentClassLogger();
        logger.Info("主窗口开始关闭流程");

        // 窗口关闭前的逻辑处理
        if (DataContext is MainWindowViewModel viewModel)
        {
            try
            {
                logger.Info("开始执行ViewModel清理逻辑");
                // 执行正常的清理逻辑
                viewModel.Cleanup();
                logger.Info("ViewModel清理逻辑执行完成");

                // 注意：这里不记录为"用户手动退出"，因为窗口关闭可能由多种原因触发
                logger.Info("窗口关闭触发的应用程序退出（非用户手动退出）");
                Application.Current.Shutdown(0);
            }
            catch (Exception ex)
            {
                // 如果清理过程中发生异常，记录日志并标记为异常退出
                logger.Error(ex, "窗口关闭清理时发生异常: {Message}", ex.Message);
                logger.Error("系统异常，强制退出应用程序");
                Application.Current.Shutdown(1);
            }
        }
        else
        {
            logger.Warn("主窗口DataContext为空，直接退出应用程序");
            Application.Current.Shutdown(1);
        }
    }

    /// <summary>
    /// 显示窗口并激活
    /// </summary>
    public void ShowAndActivate()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        Focus();
    }

    #region 快捷键处理

    /// <summary>
    /// 注册键盘快捷键
    /// </summary>
    private void RegisterKeyboardShortcuts()
    {
        // 注册全局快捷键处理
        KeyDown += MainWindow_KeyDown;
    }

    /// <summary>
    /// 键盘按键处理
    /// </summary>
    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        // 检查修饰键组合
        bool ctrlPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        bool shiftPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        bool altPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);

        // 处理快捷键
        switch (e.Key)
        {
            // F5 - 开始/停止匹配
            case Key.F5:
                if (viewModel.IsRunning)
                    viewModel.StopMatchingCommand?.Execute(null);
                else
                    viewModel.StartMatchingCommand?.Execute(null);
                e.Handled = true;
                break;

            // F6 - 暂停/继续匹配
            case Key.F6:
                viewModel.PauseResumeMatchingCommand?.Execute(null);
                e.Handled = true;
                break;

            // F12 - 截图
            case Key.F12:
                viewModel.ScreenCaptureCommand?.Execute(null);
                e.Handled = true;
                break;

            // Ctrl+O - 打开设置面板 (已移除设置窗口)
            // case Key.O when ctrlPressed:
            //     viewModel.OpenSettingsCommand?.Execute(null);
            //     e.Handled = true;
            //     break;

            // Ctrl+D - 清空匹配记录
            case Key.D when ctrlPressed:
                viewModel.ClearLogsCommand?.Execute(null);
                e.Handled = true;
                break;



            // 配置管理快捷键已移除

            // Ctrl+E - 导出日志
            case Key.E when ctrlPressed:
                viewModel.ExportLogsCommand?.Execute(null);
                e.Handled = true;
                break;

            // Escape - 停止匹配
            case Key.Escape:
                if (viewModel.IsRunning)
                {
                    viewModel.StopMatchingCommand?.Execute(null);
                    e.Handled = true;
                }
                break;
        }
    }

    #endregion
}
