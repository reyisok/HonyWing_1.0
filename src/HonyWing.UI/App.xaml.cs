using HonyWing.Core.Interfaces;
using HonyWing.Infrastructure.Services;
using HonyWing.UI.Services;
using HonyWing.UI.Views;
using HonyWing.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HonyWing.UI;

/// <summary>
/// App.xaml 的交互逻辑
/// </summary>
public partial class App : Application
{
    private IHost? _host;
    private readonly List<Process> _childProcesses = new();
    private readonly List<IDisposable> _disposableResources = new();
    private readonly CancellationTokenSource _shutdownCancellation = new();
    private readonly object _lockObject = new();
    private volatile bool _isShuttingDown = false;
    private static readonly object _staticLockObject = new();

    public static IServiceProvider? ServiceProvider { get; private set; }

    /// <summary>
    /// 注册需要在应用程序退出时清理的资源
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-16 15:30:00
    /// @version: 1.0.0
    /// </summary>
    public static void RegisterDisposableResource(IDisposable resource)
    {
        if (Current is App app && resource != null)
        {
            lock (_staticLockObject)
            {
                if (!app._isShuttingDown)
                {
                    app._disposableResources.Add(resource);
                }
            }
        }
    }

    /// <summary>
    /// 注册需要在应用程序退出时终止的子进程
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-16 15:30:00
    /// @version: 1.0.0
    /// </summary>
    public static void RegisterChildProcess(Process process)
    {
        if (Current is App app && process != null)
        {
            lock (_staticLockObject)
            {
                if (!app._isShuttingDown)
                {
                    app._childProcesses.Add(process);
                }
            }
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            // 配置 Serilog
            ConfigureNLog();
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("NLog配置完成");

            // 注册全局异常处理
            AppDomain.CurrentDomain.UnhandledException += Application_UnhandledException;
            DispatcherUnhandledException += Application_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            logger.Info("全局异常处理程序已注册");

            // 构建主机
            try
            {
                logger.Info("开始构建应用程序主机...");
                _host = CreateHostBuilder(e.Args).Build();
                logger.Info("主机构建完成，所有服务已注册");
            }
            catch (Exception hostEx)
            {
                logger.Fatal(hostEx, "构建主机时发生异常: {Message}", hostEx.Message);
                if (hostEx.InnerException != null)
                {
                    logger.Fatal(hostEx.InnerException, "构建主机时发生内部异常: {Message}", hostEx.InnerException.Message);
                }
                throw;
            }

            ServiceProvider = _host.Services;
            logger.Info("服务提供程序设置完成");

            // 初始化日志服务
            try
            {
                var logService = _host.Services.GetRequiredService<HonyWing.Core.Interfaces.ILogService>();
                logService.Initialize();
                logger.Info("日志服务初始化成功");
            }
            catch (Exception logEx)
            {
                logger.Error(logEx, "初始化日志服务失败: {Message}", logEx.Message);
            }

            // 启动主机
            try
            {
                logger.Info("正在启动应用程序主机...");
                await _host.StartAsync();
                logger.Info("主机启动完成，所有服务已初始化");

                // 记录已注册的服务信息
                var serviceProvider = _host.Services;
                logger.Debug("应用程序已注册以下服务:");
                logger.Debug("- 核心服务: IImageMatcher, IMouseService, IScreenCaptureService");
        logger.Debug("- 基础设施服务: IConfigurationService, IDpiAdaptationService");
        logger.Debug("- UI服务: INotificationService");
            }
            catch (Exception hostStartEx)
            {
                logger.Fatal(hostStartEx, "启动主机时发生异常: {Message}", hostStartEx.Message);
                if (hostStartEx.InnerException != null)
                {
                    logger.Fatal(hostStartEx.InnerException, "启动主机时发生内部异常: {Message}", hostStartEx.InnerException.Message);
                }
                throw;
            }

            // 创建并显示主窗口
            MainWindow? mainWindow = null;
            bool mainWindowStartupSuccess = false;

            try
            {
                logger.Info("开始创建主窗口...");
                mainWindow = _host.Services.GetRequiredService<MainWindow>();
                logger.Info("主窗口创建成功，窗口标题: {Title}", mainWindow.Title);
            }
            catch (Exception createEx)
            {
                logger.Fatal(createEx, "创建主窗口时发生异常: {Message}", createEx.Message);
                if (createEx.InnerException != null)
                {
                    logger.Fatal(createEx.InnerException, "创建主窗口时发生内部异常: {Message}", createEx.InnerException.Message);
                }
                logger.Fatal("主程序主界面未正常启动，系统异常，应用程序将退出");
                throw;
            }

            try
            {
                // 确保在UI线程上执行Show操作
                if (mainWindow != null)
                {
                    logger.Info("准备显示主窗口...");

                    Dispatcher.Invoke(() => {
                        try
                        {
                            logger.Info("开始在UI线程上显示主窗口...");

                            mainWindow.Show();
                            mainWindow.Activate();

                            // 验证主窗口是否真正显示
                            if (mainWindow.IsVisible && mainWindow.IsLoaded)
                            {
                                mainWindowStartupSuccess = true;
                                logger.Info("主窗口显示和加载成功");
                            }
                            else
                            {
                                logger.Error("主窗口已创建但未正确显示或加载");
                                throw new InvalidOperationException("主窗口未能正确显示");
                            }
                        }
                        catch (Exception innerEx)
                        {
                            logger.Error(innerEx, "在UI线程上显示主窗口时发生异常: {Message}", innerEx.Message);
                            throw;
                        }
                    });
                }
                else
                {
                    throw new InvalidOperationException("主窗口对象为空，无法显示");
                }
            }
            catch (Exception showEx)
            {
                logger.Error(showEx, "显示主窗口时发生异常: {Message}", showEx.Message);
                if (showEx.InnerException != null)
                {
                    logger.Error(showEx.InnerException, "显示主窗口时发生内部异常: {Message}", showEx.InnerException.Message);
                }
                logger.Fatal("主程序主界面未正常启动，系统异常，应用程序将退出");
                mainWindowStartupSuccess = false;
                throw;
            }

            // 检查主窗口启动状态
            if (!mainWindowStartupSuccess)
            {
                logger.Fatal("主程序主界面未正常启动，系统异常，终止应用程序进程");
                // 强制终止所有相关进程
                try
                {
                    var currentProcess = Process.GetCurrentProcess();
                    logger.Info("正在终止当前进程: {ProcessName} (PID: {ProcessId})", currentProcess.ProcessName, currentProcess.Id);
                    Environment.Exit(1);
                }
                catch (Exception terminateEx)
                {
                    logger.Fatal(terminateEx, "终止进程时发生异常: {Message}", terminateEx.Message);
                    Environment.Exit(1);
                }
            }

            logger.Info("HonyWing应用程序启动成功完成");
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Fatal(ex, "应用程序启动失败: {Message}", ex.Message);
            logger.Fatal("应用程序启动失败，详细错误信息: {ExceptionDetails}", ex.ToString());

            // 记录内部异常信息（如果有）
            if (ex.InnerException != null)
            {
                logger.Fatal(ex.InnerException, "应用程序启动内部异常: {Message}", ex.InnerException.Message);

                // 记录更深层的内部异常（如果有）
                var innerEx = ex.InnerException.InnerException;
                if (innerEx != null)
                {
                    logger.Fatal(innerEx, "应用程序启动深层内部异常: {Message}", innerEx.Message);
                }
            }

            // 记录当前环境信息，有助于诊断问题
            logger.Fatal("应用程序启动失败环境信息: OS={OS}, .NET版本={DotNetVersion}, 内存={Memory}MB",
                Environment.OSVersion,
                Environment.Version,
                Environment.WorkingSet / (1024 * 1024));

            // 启动失败已通过日志记录

            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        var logger = NLog.LogManager.GetCurrentClassLogger();
        logger.Info("开始应用程序退出流程");

        lock (_lockObject)
        {
            _isShuttingDown = true;
        }

        try
        {
            // 设置关闭超时时间
            _shutdownCancellation.CancelAfter(TimeSpan.FromSeconds(10));

            // 1. 停止主机服务
            await StopHostAsync();

            // 2. 清理子进程
            await CleanupChildProcessesAsync();

            // 3. 清理可释放资源
            CleanupDisposableResources();

            // 4. 强制垃圾回收
            ForceGarbageCollection();

            logger.Info("应用程序资源清理完成");
        }
        catch (Exception ex)
        {
            // 记录退出时的异常，但不阻止应用程序退出
            logger.Error(ex, "应用程序退出时发生异常: {Message}", ex.Message);
        }
        finally
        {
            try
            {
                _shutdownCancellation?.Dispose();
            }
            catch { /* 忽略释放异常 */ }

            logger.Info("应用程序退出流程完成");
            NLog.LogManager.Shutdown();
            base.OnExit(e);
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // 注册核心服务
                services.AddSingleton<IPathService, PathService>();
                services.AddSingleton<IImageMatcher, ImageMatcherService>();
                services.AddSingleton<IMouseService, MouseService>();
                services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
                services.AddSingleton<IConfigurationService, ConfigurationService>();
                services.AddSingleton<IImageService, ImageService>();
                services.AddSingleton<IImageTemplateService, ImageTemplateService>();
                services.AddSingleton<IClickAnimationService, ClickAnimationService>();

                services.AddSingleton<IDpiAdaptationService, DpiAdaptationService>();
                services.AddSingleton<IGlobalHotkeyService, GlobalHotkeyService>();

                services.AddSingleton<INotificationService, DialogService>();

                // 资源管理服务已移除

                // 注册视图模型
                    services.AddTransient<MainWindowViewModel>();
                    // SettingsViewModel已移除，设置功能已整合到主界面

                    // 注册应用设置服务
                    services.AddSingleton<IAppSettingsService, AppSettingsService>();

                // 注册视图
                services.AddTransient<MainWindow>();
                // SettingsWindow已移除，设置功能已整合到主界面

                // 注册新的日志服务
                services.AddSingleton<HonyWing.Core.Interfaces.ILogService>(provider =>
                {
                    var logService = new HonyWing.Infrastructure.Services.LogService(provider.GetRequiredService<ILogger<HonyWing.Infrastructure.Services.LogService>>());
                    RegisterDisposableResource(logService);
                    return logService;
                });

                // 注册日志服务
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddNLog();
                });
            });

    private static void ConfigureNLog()
    {
        // 按项目规则要求设置日志路径：项目根目录下的\logs文件夹
        var projectRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));
        if (string.IsNullOrEmpty(projectRoot))
        {
            projectRoot = AppDomain.CurrentDomain.BaseDirectory;
        }
        var logDirectory = Path.Combine(projectRoot, "logs");
        Directory.CreateDirectory(logDirectory);

        // 设置NLog变量，让NLog.config文件中的变量能够正确解析
        NLog.GlobalDiagnosticsContext.Set("logDirectory", logDirectory);
        
        // 确保各个日志子目录存在
        Directory.CreateDirectory(Path.Combine(logDirectory, "app"));
        Directory.CreateDirectory(Path.Combine(logDirectory, "image"));
        Directory.CreateDirectory(Path.Combine(logDirectory, "error"));
        Directory.CreateDirectory(Path.Combine(logDirectory, "performance"));
        Directory.CreateDirectory(Path.Combine(logDirectory, "system"));
        Directory.CreateDirectory(Path.Combine(logDirectory, "debug"));
        Directory.CreateDirectory(Path.Combine(logDirectory, "build"));

        // NLog会自动加载NLog.config文件，不需要手动配置
        // 只需要确保配置文件路径正确
        var logger = NLog.LogManager.GetCurrentClassLogger();
        logger.Info("HonyWing 应用程序启动");
        logger.Info($"日志文件保存路径: {logDirectory}");
        logger.Info("NLog配置已从NLog.config文件加载，支持分类日志记录");
    }

    private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = NLog.LogManager.GetCurrentClassLogger();
        logger.Fatal(e.Exception, "未处理的UI线程异常: {0}", e.Exception.Message);
        logger.Fatal("UI线程异常详细信息: {0}", e.Exception.ToString());

        // 记录内部异常信息（如果有）
        if (e.Exception.InnerException != null)
        {
            logger.Fatal(e.Exception.InnerException, "UI线程内部异常: {0}", e.Exception.InnerException.Message);
        }

        // 显示友好的错误消息
        // 错误已通过日志记录，移除MessageBox弹窗

        // 标记异常已处理，应用程序将继续运行
        e.Handled = true;

        logger.Warn("应用程序将继续运行，但可能不稳定");
    }

    /// <summary>
    /// 停止主机服务
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-16 15:30:00
    /// @version: 1.0.0
    /// </summary>
    private async Task StopHostAsync()
    {
        if (_host != null)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("正在停止主机服务...");
            try
            {
                await _host.StopAsync(_shutdownCancellation.Token);
                _host.Dispose();
                logger.Info("主机服务已停止");
            }
            catch (OperationCanceledException)
            {
                logger.Warn("主机服务停止超时");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "停止主机服务时发生异常");
            }
        }
    }

    /// <summary>
    /// 清理子进程
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-16 15:30:00
    /// @version: 1.0.0
    /// </summary>
    private async Task CleanupChildProcessesAsync()
    {
        if (_childProcesses.Count > 0)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("正在清理 {Count} 个子进程...", _childProcesses.Count);

            var tasks = new List<Task>();
            foreach (var process in _childProcesses.ToArray())
            {
                tasks.Add(Task.Run(() => CleanupSingleProcess(process)));
            }

            try
            {
                await Task.WhenAll(tasks).WaitAsync(_shutdownCancellation.Token);
                logger.Info("所有子进程已清理完成");
            }
            catch (OperationCanceledException)
            {
                logger.Warn("子进程清理超时");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "清理子进程时发生异常");
            }
        }
    }

    /// <summary>
    /// 清理单个进程
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-16 15:30:00
    /// @version: 1.0.0
    /// </summary>
    private void CleanupSingleProcess(Process process)
    {
        var logger = NLog.LogManager.GetCurrentClassLogger();
        try
        {
            if (process != null && !process.HasExited)
            {
                logger.Info("正在终止进程: {ProcessName} (PID: {ProcessId})", process.ProcessName, process.Id);

                // 尝试优雅关闭
                process.CloseMainWindow();

                // 等待进程退出
                if (!process.WaitForExit(3000))
                {
                    // 强制终止
                    process.Kill();
                    process.WaitForExit(2000);
                }

                logger.Info("进程已终止: {ProcessName}", process.ProcessName);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "清理进程时发生异常: {ProcessName}", process?.ProcessName ?? "Unknown");
        }
        finally
        {
            try
            {
                process?.Dispose();
            }
            catch { /* 忽略释放异常 */ }
        }
    }

    /// <summary>
    /// 清理可释放资源
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-16 15:30:00
    /// @version: 1.0.0
    /// </summary>
    private void CleanupDisposableResources()
    {
        if (_disposableResources.Count > 0)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("正在清理 {Count} 个可释放资源...", _disposableResources.Count);

            foreach (var resource in _disposableResources.ToArray())
            {
                try
                {
                    resource?.Dispose();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "释放资源时发生异常: {ResourceType}", resource?.GetType().Name ?? "Unknown");
                }
            }

            _disposableResources.Clear();
            logger.Info("可释放资源清理完成");
        }
    }

    /// <summary>
    /// 强制垃圾回收
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-16 15:30:00
    /// @version: 1.0.0
    /// </summary>
    private void ForceGarbageCollection()
    {
        var logger = NLog.LogManager.GetCurrentClassLogger();
        try
        {
            logger.Info("正在执行垃圾回收...");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            logger.Info("垃圾回收完成");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "执行垃圾回收时发生异常");
        }
    }

    private void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var logger = NLog.LogManager.GetCurrentClassLogger();
        if (e.ExceptionObject is Exception ex)
        {
            logger.Fatal(ex, "未处理的应用程序异常: {Message}", ex.Message);
            logger.Fatal("应用程序异常详细信息: {ExceptionDetails}", ex.ToString());
            logger.Fatal("异常是否终止应用程序: {IsTerminating}", e.IsTerminating);
        }
        else
        {
            logger.Fatal("未处理的非托管异常: {ExceptionObject}", e.ExceptionObject?.ToString() ?? "Unknown");
        }

        logger.Fatal("应用程序将因严重错误而退出");
        // 严重错误已通过日志记录，移除MessageBox弹窗
        Shutdown(1);
    }

    /// <summary>
    /// 处理未观察的任务异常
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-09-06 00:30:00
    /// @version: 1.0.0
    /// </summary>
    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var logger = NLog.LogManager.GetCurrentClassLogger();
        if (e.Exception != null)
        {
            logger.Error(e.Exception, "未观察的任务异常: {Message}", e.Exception.Message);
            foreach (var innerEx in e.Exception.InnerExceptions)
            {
                logger.Error(innerEx, "任务内部异常: {Message}", innerEx.Message);
            }
        }

        // 标记异常已观察，防止进程崩溃
        e.SetObserved();
    }
}
