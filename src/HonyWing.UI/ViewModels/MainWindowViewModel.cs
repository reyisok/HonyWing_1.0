using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using HonyWing.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using HonyWing.UI.Commands;
using System.Windows;
using Microsoft.Win32;
using HonyWing.UI.Views;
using System.IO;
using System.Drawing.Imaging;
using DpiChangedEventArgs = HonyWing.Core.Interfaces.DpiChangedEventArgs;
using HonyWing.Core.Interfaces;
using HonyWing.UI.ViewModels;
using NLog;

namespace HonyWing.UI.ViewModels;

/// <summary>
/// 主窗口视图模型
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IImageMatcher _imageMatcher;
    private readonly IMouseService _mouseService;
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly IConfigurationService _configurationService;


    private readonly IImageService _imageService;
    private readonly IDpiAdaptationService _dpiAdaptationService;

    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly INotificationService _notificationService;
    private readonly HonyWing.Core.Interfaces.ILogService _logService;
    private readonly IGlobalHotkeyService _globalHotkeyService;

    [ObservableProperty]
    private string _selectedImagePath = string.Empty;

    [ObservableProperty]
    private string _selectedImageFileName = "未选择图片";

    [ObservableProperty]
    private string _referenceImagePath = string.Empty;

    /// <summary>
    /// 参照图片对象
    /// </summary>
    public System.Drawing.Bitmap? ReferenceImage { get; private set; }



    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private Brush _statusColor = Brushes.Green;

    [ObservableProperty]
    private string _matchResult = "无";

    [ObservableProperty]
    private string _executionTime = "0ms";

    [ObservableProperty]
    private string _statusBarText = "就绪";

    [ObservableProperty]
    private DateTime _currentTime = DateTime.Now;









    // 监控区域设置相关属性
    private bool _isFullScreenMonitoring = true;
    public bool IsFullScreenMonitoring
    {
        get => _isFullScreenMonitoring;
        set
        {
            if (SetProperty(ref _isFullScreenMonitoring, value) && value)
            {
                IsCustomAreaMonitoring = false;
                MonitoringAreaInfo = "全屏监控";
                HasSelectedArea = false;
                SelectedMonitoringArea = null;

            }
        }
    }

    private bool _isCustomAreaMonitoring = false;
    public bool IsCustomAreaMonitoring
    {
        get => _isCustomAreaMonitoring;
        set
        {
            if (SetProperty(ref _isCustomAreaMonitoring, value) && value)
            {
                IsFullScreenMonitoring = false;
                if (!HasSelectedArea)
                {
                    MonitoringAreaInfo = "请选择监控区域";
                }

            }
        }
    }

    [ObservableProperty]
    private string _monitoringAreaInfo = "全屏监控";

    [ObservableProperty]
    private bool _hasSelectedArea = false;

    private Rect? _selectedMonitoringArea;
    public Rect? SelectedMonitoringArea
    {
        get => _selectedMonitoringArea;
        set => SetProperty(ref _selectedMonitoringArea, value);
    }

    // 匹配与点击控制相关属性
    [ObservableProperty]
    private double _matchThreshold = 0.8;

    // 点击类型单选按钮属性
    [ObservableProperty]
    private bool _isLeftClick = true;

    [ObservableProperty]
    private bool _isLeftDoubleClick = false;

    [ObservableProperty]
    private bool _isRightClick = false;

    // 获取当前选中的点击类型
    public string ClickType
    {
        get
        {
            if (IsLeftClick) return "左键单击";
            if (IsLeftDoubleClick) return "左键双击";
            if (IsRightClick) return "右键单击";
            return "左键单击"; // 默认值
        }
    }

    // 时间控制相关属性
    [ObservableProperty]
    private string _monitoringMode = "实时监控";

    [ObservableProperty]
    private bool _isTimedMonitoring = false;

    [ObservableProperty]
    private DateTime? _startDate = DateTime.Today;

    [ObservableProperty]
    private string _startTime = "00:00:00";

    [ObservableProperty]
    private string _endTime = "23:59:59";

    private int _matchInterval = 900;
    public int MatchInterval
    {
        get => _matchInterval;
        set
        {
            if (SetProperty(ref _matchInterval, value))
            {
                OnMatchIntervalChanged(value);
            }
        }
    }

    [ObservableProperty]
    private string _matchIntervalDisplay = "0.9 秒";

    // 新增监控间隔属性（秒）
    [ObservableProperty]
    private double _monitorInterval = 2.0;

    // 新增点击延迟属性（毫秒）
    [ObservableProperty]
    private int _clickDelay = 900;

    /// <summary>
    /// 当MatchInterval属性变化时自动更新显示
    /// </summary>
    private void OnMatchIntervalChanged(int value)
    {
        // 更新时间间隔显示
        if (value >= 1000)
        {
            var seconds = value / 1000.0;
            MatchIntervalDisplay = seconds == (int)seconds ? $"{(int)seconds} 秒" : $"{seconds:F1} 秒";
        }
        else
        {
            MatchIntervalDisplay = $"{value} 毫秒";
        }
    }

    // 移除循环次数相关属性，系统默认无限循环

    [ObservableProperty]
    private string _runningTimeDisplay = "00:00:00";

    // DPI适配相关属性
    [ObservableProperty]
    private double _systemDpiX = 96.0;

    [ObservableProperty]
    private double _systemDpiY = 96.0;

    [ObservableProperty]
    private double _dpiScaleX = 1.0;

    [ObservableProperty]
    private double _dpiScaleY = 1.0;

    [ObservableProperty]
    private string _dpiInfo = "DPI: 96x96 (100%)";

    // 匹配统计相关属性
    [ObservableProperty]
    private int _totalMatchAttempts = 0;

    [ObservableProperty]
    private int _successfulMatches = 0;

    [ObservableProperty]
    private double _matchSuccessRate = 0.0;

    [ObservableProperty]
    private ObservableCollection<MatchRecord> _matchHistory = new();

    [ObservableProperty]
    private string _matchStatistics = "匹配统计: 0次尝试, 0次成功, 成功率: 0%";

    // 日志查看器ViewModel
    [ObservableProperty]
    private LogViewerViewModel _logViewerViewModel;

    // 系统配置相关属性




    // 图像匹配设置属性
    [ObservableProperty]
    private int _timeoutMs = 5000;

    [ObservableProperty]
    private int _searchIntervalMs = 100;

    [ObservableProperty]
    private bool _enableMultiScale = true;

    // 鼠标操作设置属性
    [ObservableProperty]
    private int _clickDelayMs = 50;

    [ObservableProperty]
    private double _moveSpeed = 2.0;

    [ObservableProperty]
    private bool _enableSmoothMove = true;

    [ObservableProperty]
    private int _clickHoldDurationMs = 100;

    [ObservableProperty]
    private int _multiTargetIntervalMs = 500;





    // 运行状态属性
    public bool IsRunning => _isMatching;
    public bool IsPaused => _isPaused;

    private bool _isMatching = false;
    private bool _isPaused = false;
    private CancellationTokenSource? _matchingCancellationTokenSource;
    private readonly Timer _timeUpdateTimer;
    private DateTime _matchingStartTime;
    // 移除循环计数，系统默认无限循环
    private Timer? _matchingTimer;

    public MainWindowViewModel(
        IImageMatcher imageMatcher,
        IMouseService mouseService,
        IScreenCaptureService screenCaptureService,
        IConfigurationService configurationService,

        IImageService imageService,
        IDpiAdaptationService dpiAdaptationService,
        ILogger<MainWindowViewModel> logger,
        INotificationService notificationService,
        HonyWing.Core.Interfaces.ILogService logService,
        IGlobalHotkeyService globalHotkeyService,
        IServiceProvider serviceProvider)
    {
        _imageMatcher = imageMatcher;
        _mouseService = mouseService;
        _screenCaptureService = screenCaptureService;
        _configurationService = configurationService;

        _imageService = imageService;
        _dpiAdaptationService = dpiAdaptationService;

        _logger = logger;
        _notificationService = notificationService;
        _logService = logService;
        _globalHotkeyService = globalHotkeyService;

        // 初始化日志查看器ViewModel
        var logViewerLogger = serviceProvider.GetRequiredService<ILogger<LogViewerViewModel>>();
        _logViewerViewModel = new LogViewerViewModel(_logService, logViewerLogger);

        // 初始化命令
        InitializeCommands();

        // 启动时间更新定时器
        _timeUpdateTimer = new Timer(UpdateCurrentTime, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        // 初始化DPI信息
        InitializeDpiInfo();

        // 订阅DPI变化事件
        _dpiAdaptationService.DpiChanged += OnDpiChanged;

    }



    #region Commands

    // 文件菜单命令
    public ICommand ExitCommand { get; private set; } = null!;

    // 编辑菜单命令
    // OpenSettingsCommand已移除，设置功能已整合到主界面

    // 工具菜单命令
    public ICommand ScreenCaptureCommand { get; private set; } = null!;
    public ICommand AreaSelectionCommand { get; private set; } = null!;

    // 监控区域命令




    // 图片选择命令
    public ICommand SelectReferenceImageCommand { get; private set; } = null!;

    // 操作控制命令
    public ICommand StartMatchingCommand { get; private set; } = null!;
    public ICommand StopMatchingCommand { get; private set; } = null!;
    public ICommand PauseResumeMatchingCommand { get; private set; } = null!;

    // 日志管理命令
    public ICommand ClearLogsCommand { get; private set; } = null!;
    public ICommand ExportLogsCommand { get; private set; } = null!;

    // 配置管理命令已移除

    public ICommand OpenLogFolderCommand { get; private set; } = null!;









    private void Exit()
    {
        _logger.LogInformation("用户手动退出应用程序");

        try
        {
            _logger.LogDebug("开始执行应用程序退出清理操作");
            // 执行清理操作
            Cleanup();

            _logger.LogInformation("用户手动退出 - 应用程序清理完成，正常退出");
            // 正常退出应用程序，退出代码为0
            System.Windows.Application.Current.Shutdown(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户手动退出时发生异常: {Message}", ex.Message);
            _logger.LogError("用户手动退出异常，退出代码: 1");

            // 异常退出，退出代码为1
            System.Windows.Application.Current.Shutdown(1);
        }
    }

    // OpenSettings方法已移除，设置功能已整合到主界面

    private async Task StartMatching()
    {
        if (_isMatching)
        {
            _logger.LogWarning("尝试启动匹配，但匹配已在运行中");
            return;
        }

        try
        {
            _logger.LogInformation("准备启动图像匹配");

            // 检查定时监控设置
            if (IsTimedMonitoring && !IsValidTimedMonitoring())
            {
                _logger.LogWarning("定时监控设置无效，无法启动匹配");
                return;
            }

            _isMatching = true;
            _isPaused = false;
            _matchingCancellationTokenSource = new CancellationTokenSource();
            _matchingStartTime = DateTime.Now;
            _hasShownStartReminder = false; // 重置定时启动提醒标志

            StatusText = "匹配中";
            StatusColor = Brushes.Orange;
            StatusBarText = "正在进行图像匹配...";

            // 记录开始匹配日志
            _logger.LogInformation("开始图像匹配 - 模式: 定时监控, 监控间隔: {MonitorInterval}秒, 匹配阈值: {MatchThreshold:P1}, 点击延迟: {ClickDelay}ms",
                MonitorInterval, MatchThreshold, ClickDelay);

            // 通知UI更新
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(IsPaused));

            // 启动运行时间更新定时器
            _matchingTimer = new Timer(UpdateRunningTime, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            _logger.LogDebug("运行时间更新定时器已启动");

            // 开始匹配循环
            _logger.LogDebug("开始匹配循环");
            await StartMatchingLoop(_matchingCancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动图像匹配失败");
            _isMatching = false;

            StatusText = "匹配错误";
            StatusColor = Brushes.Red;
            MatchResult = "匹配过程中发生错误";
        }
        finally
        {
            _isMatching = false;
            _matchingTimer?.Dispose();
            _matchingTimer = null;
            _matchingCancellationTokenSource?.Dispose();
            _matchingCancellationTokenSource = null;

            StatusBarText = "就绪";
            RunningTimeDisplay = "00:00:00";
        }
    }

    private async Task StartMatchingLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // 检查是否暂停
                if (_isPaused)
                {
                    await Task.Delay(100, cancellationToken); // 暂停时短暂等待
                    continue;
                }

                // 检查是否为定时监控且不在监控时间范围内
                if (IsTimedMonitoring && !IsInMonitoringTimeRange())
                {
                    // 检查是否需要显示定时启动前10秒提醒
                    CheckAndShowTimedStartReminder();

                    await Task.Delay(1000, cancellationToken); // 等待1秒后再检查
                    continue;
                }

                // 执行单次匹配
                await PerformSingleMatch();

                // 等待指定的监控间隔（转换为毫秒）
                await Task.Delay((int)(MonitorInterval * 1000), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "匹配循环中发生错误");


                // 等待一段时间后继续
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private async Task PerformSingleMatch()
    {
        var startTime = DateTime.Now;
        var matchRecord = new MatchRecord
        {
            Timestamp = startTime,
            TemplateName = "参照图片",
            ClickType = ClickType
        };

        try
        {
            _logger.LogDebug("开始执行单次图像匹配");

            // 执行图像匹配
            if (ReferenceImage != null)
            {
                // 使用当前设置的相似度阈值
                var threshold = MatchThreshold;
                _logger.LogDebug("开始图像匹配，阈值: {Threshold:P1}", threshold);

                var result = await _imageMatcher.FindImageAsync(ReferenceImage, threshold);

                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                ExecutionTime = $"{duration.TotalMilliseconds:F0}ms";
                matchRecord.ExecutionTimeMs = duration.TotalMilliseconds;

                // 更新匹配统计
                TotalMatchAttempts++;

                if (result != null)
                {
                    // 匹配成功
                    SuccessfulMatches++;
                    matchRecord.IsSuccess = true;
                    matchRecord.Similarity = result.Similarity;
                    matchRecord.MatchLocation = result.Location;

                    _logger.LogInformation("图像匹配成功 - 位置: ({X}, {Y}), 相似度: {Similarity:P2}, 执行时间: {ExecutionTime}ms",
                        result.Location.X, result.Location.Y, result.Similarity, duration.TotalMilliseconds);

                    MatchResult = $"找到匹配 ({result.Location.X}, {result.Location.Y}) 相似度: {result.Similarity:P1}";
                    StatusText = "匹配成功";
                    StatusColor = Brushes.Green;

                    // 记录匹配成功日志
                    _logger.LogInformation($"匹配成功 - 位置: ({result.Location.X}, {result.Location.Y}), 相似度: {result.Similarity:P1}, 执行时间: {duration.TotalMilliseconds:F0}ms");



                    // 计算图像中心点坐标进行精确点击
                    var centerX = result.Location.X + (result.Rectangle.Width / 2);
                    var centerY = result.Location.Y + (result.Rectangle.Height / 2);
                    var clickPoint = new System.Drawing.Point(centerX, centerY);
                    matchRecord.ClickLocation = clickPoint;
                    
                    _logger.LogDebug("计算点击中心点: 匹配位置({MatchX}, {MatchY}), 图像尺寸({Width}x{Height}), 中心点({CenterX}, {CenterY})",
                        result.Location.X, result.Location.Y, result.Rectangle.Width, result.Rectangle.Height, centerX, centerY);

                    // 设置点击延迟
                    _mouseService.SetClickDelay(ClickDelay);

                    // 根据点击类型执行相应的点击操作
                    switch (ClickType)
                    {
                        case "左键单击":
                            await _mouseService.LeftClickAsync(clickPoint);
                            break;
                        case "左键双击":
                            await _mouseService.LeftDoubleClickAsync(clickPoint);
                            break;
                        case "右键单击":
                            await _mouseService.RightClickAsync(clickPoint);
                            break;
                        case "中键单击":
                            await _mouseService.MiddleClickAsync(clickPoint);
                            break;
                        default:
                            await _mouseService.LeftClickAsync(clickPoint);
                            break;
                    }



                    // 参照图片匹配成功
                }
                else
                {
                    // 匹配失败
                    matchRecord.IsSuccess = false;
                    matchRecord.Similarity = 0.0;

                    _logger.LogDebug("图像匹配失败 - 未找到符合阈值 {Threshold:P1} 的匹配项, 执行时间: {ExecutionTime}ms",
                        threshold, duration.TotalMilliseconds);

                    MatchResult = "未找到匹配";
                    StatusText = "匹配中";
                    StatusColor = Brushes.Orange;
                }
            }
            else
            {
                // 模板加载失败
                matchRecord.IsSuccess = false;
                matchRecord.ErrorMessage = "无法加载模板图像";
                TotalMatchAttempts++;

                _logger.LogWarning("模板图像未加载，无法执行匹配");
            }
        }
        catch (Exception ex)
        {
            // 匹配过程中发生异常
            matchRecord.IsSuccess = false;
            matchRecord.ErrorMessage = ex.Message;
            TotalMatchAttempts++;

            _logger.LogError(ex, "执行图像匹配时发生异常");
        }
        finally
        {
            // 添加匹配记录到历史
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            MatchHistory.Add(matchRecord);

            // 限制历史记录数量
            while (MatchHistory.Count > 500)
            {
                MatchHistory.RemoveAt(0);
            }

            // 更新统计信息
            UpdateMatchStatistics();
        });
        }
    }

    private bool IsValidTimedMonitoring()
    {
        if (!StartDate.HasValue)
            return false;

        if (!TimeSpan.TryParse(StartTime, out var startTimeSpan) ||
            !TimeSpan.TryParse(EndTime, out var endTimeSpan))
            return false;

        return startTimeSpan < endTimeSpan;
    }

    private bool IsInMonitoringTimeRange()
    {
        if (!IsTimedMonitoring || !StartDate.HasValue)
            return true;

        var now = DateTime.Now;
        var startDate = StartDate.Value;

        // 检查日期
        if (now.Date < startDate.Date)
            return false;

        if (!TimeSpan.TryParse(StartTime, out var startTimeSpan) ||
            !TimeSpan.TryParse(EndTime, out var endTimeSpan))
            return true;

        var currentTime = now.TimeOfDay;

        // 检查时间范围
        return currentTime >= startTimeSpan && currentTime <= endTimeSpan;
    }

    private bool _hasShownStartReminder = false;

    /// <summary>
    /// 检查并显示定时启动前10秒提醒
    /// </summary>
    private void CheckAndShowTimedStartReminder()
    {
        if (!IsTimedMonitoring || !StartDate.HasValue || _hasShownStartReminder)
            return;

        try
        {
            var now = DateTime.Now;
            var startDate = StartDate.Value;

            // 只在启动日期当天检查
            if (now.Date != startDate.Date)
                return;

            if (!TimeSpan.TryParse(StartTime, out var startTimeSpan))
                return;

            // 计算启动时间
            var startDateTime = startDate.Date.Add(startTimeSpan);
            var timeUntilStart = startDateTime - now;

            // 检查是否在启动前10秒内
            if (timeUntilStart.TotalSeconds > 0 && timeUntilStart.TotalSeconds <= 10)
            {
                _hasShownStartReminder = true;
                var remainingSeconds = (int)Math.Ceiling(timeUntilStart.TotalSeconds);




            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查定时启动提醒失败");
        }
    }

    private void UpdateRunningTime(object? state)
    {
        if (_isMatching)
        {
            var elapsed = DateTime.Now - _matchingStartTime;
            RunningTimeDisplay = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
        }
    }

    private void StopMatching()
    {
        if (!_isMatching)
        {
            _logger.LogWarning("尝试停止匹配，但匹配未在运行中");
            return;
        }

        _logger.LogInformation("用户请求停止图像匹配");

        try
        {
            _logger.LogDebug("正在取消匹配操作");
            _matchingCancellationTokenSource?.Cancel();

            // 重置状态
            _isMatching = false;
            _isPaused = false;

            StatusText = "已停止";
            StatusColor = Brushes.Gray;
            StatusBarText = "操作已停止";

            // 记录停止匹配日志
            var elapsed = DateTime.Now - _matchingStartTime;
            _logger.LogInformation("图像匹配已停止 - 运行时间: {RunTime}, 总尝试: {TotalAttempts}次, 成功: {SuccessfulMatches}次, 成功率: {SuccessRate:F1}%",
                $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}", TotalMatchAttempts, SuccessfulMatches,
                TotalMatchAttempts > 0 ? (double)SuccessfulMatches / TotalMatchAttempts * 100 : 0);

            // 停止运行时间更新
            _logger.LogDebug("正在释放匹配定时器资源");
            _matchingTimer?.Dispose();
            _matchingTimer = null;

            // 系统默认无限循环，无需重置计数

            // 通知UI更新
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(IsPaused));

            _logger.LogDebug("匹配停止操作完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止匹配时发生异常");
        }
    }

    private void PauseResumeMatching()
    {
        if (!_isMatching)
        {
            _logger.LogWarning("尝试暂停/恢复匹配，但匹配未在运行中");
            return;
        }

        _logger.LogInformation("用户请求{Action}图像匹配", _isPaused ? "恢复" : "暂停");

        try
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                StatusText = "已暂停";
                StatusColor = Brushes.Yellow;
                StatusBarText = "匹配已暂停";

                // 记录暂停日志
                var elapsed = DateTime.Now - _matchingStartTime;
                _logger.LogInformation("图像匹配已暂停 - 运行时间: {RunTime}", $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}");
            }
            else
            {
                StatusText = "匹配中";
                StatusColor = Brushes.Orange;
                StatusBarText = "正在进行图像匹配...";

                // 记录恢复日志
                _logger.LogInformation("图像匹配已恢复");
            }

            // 通知UI更新
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(IsPaused));

            _logger.LogDebug("暂停/恢复操作完成，当前状态: {Status}", _isPaused ? "暂停" : "运行中");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "暂停/恢复匹配时发生异常");
        }
    }

    private async Task ScreenCapture()
    {
        _logger.LogInformation("用户请求执行屏幕截图操作");
        try
        {
            _logger.LogDebug("调用屏幕截图服务进行全屏截图");

            var screenshot = await _screenCaptureService.CaptureFullScreenAsync();
            if (screenshot != null)
            {
                _logger.LogInformation("屏幕截图成功，图像尺寸: {Width}x{Height}", screenshot.Width, screenshot.Height);

                // 打开保存文件对话框
                _logger.LogDebug("打开文件保存对话框");
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "保存截图",
                    Filter = "PNG文件|*.png|JPEG文件|*.jpg|BMP文件|*.bmp|所有文件|*.*",
                    FilterIndex = 1,
                    FileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var filePath = saveFileDialog.FileName;
                    var extension = System.IO.Path.GetExtension(filePath).ToLower();
                    _logger.LogDebug("用户选择保存路径: {FilePath}, 文件格式: {Extension}", filePath, extension);

                    // 根据文件扩展名选择编码器
                    BitmapEncoder encoder = extension switch
                    {
                        ".jpg" or ".jpeg" => new JpegBitmapEncoder(),
                        ".bmp" => new BmpBitmapEncoder(),
                        _ => new PngBitmapEncoder()
                    };

                    _logger.LogDebug("开始保存截图文件，使用编码器: {EncoderType}", encoder.GetType().Name);

                    // 将System.Drawing.Bitmap转换为BitmapSource
                    using (var memory = new MemoryStream())
                    {
                        screenshot.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                        memory.Position = 0;

                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memory;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        _logger.LogDebug("正在写入截图文件到磁盘");
                        encoder.Save(fileStream);
                    }

                    _logger.LogInformation("截图已成功保存到文件: {FilePath}", filePath);
                }
                else
                {
                    _logger.LogInformation("用户取消了截图保存操作");
                }
            }
            else
            {
                _logger.LogWarning("屏幕截图失败，返回的图像为空");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "屏幕截图失败");

        }
    }

    private async Task ExportLogs()
    {
        try
        {
            _logger.LogInformation("用户请求导出日志");

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "导出日志",
                Filter = "CSV文件|*.csv|文本文件|*.txt|所有文件|*.*",
                FilterIndex = 1,
                FileName = $"HonyWing_Logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            _logger.LogDebug("打开日志导出文件保存对话框");

            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;
                var isCSV = Path.GetExtension(filePath).ToLower() == ".csv";
                var logCount = LogViewerViewModel.LogEntries.Count;

                _logger.LogDebug("用户选择导出路径: {FilePath}, 格式: {Format}, 日志条数: {LogCount}",
                    filePath, isCSV ? "CSV" : "TXT", logCount);

                using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    _logger.LogDebug("开始写入日志文件");

                    if (isCSV)
                    {
                        // CSV格式
                        await writer.WriteLineAsync("时间,级别,消息");
                        foreach (var entry in LogViewerViewModel.LogEntries)
                        {
                            var csvLine = $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss},{entry.Level},\"{entry.Message.Replace("\"", "\"\"")}\"";
                            await writer.WriteLineAsync(csvLine);
                        }
                    }
                    else
                    {
                        // 文本格式
                        await writer.WriteLineAsync($"HonyWing 运行日志 - 导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        await writer.WriteLineAsync(new string('=', 60));
                        await writer.WriteLineAsync();

                        foreach (var entry in LogViewerViewModel.LogEntries)
                        {
                            await writer.WriteLineAsync($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] {entry.Message}");
                        }
                    }
                }

                _logger.LogInformation("日志导出成功: {FilePath}, 导出条数: {LogCount}", filePath, logCount);
            }
            else
            {
                _logger.LogInformation("用户取消了日志导出操作");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出日志失败");

        }
    }

    private void AreaSelection()
    {
        try
        {
            _logger.LogInformation("用户请求进行区域选择操作");

            // 使用区域选择窗口进行屏幕框选
            _logger.LogDebug("正在创建区域选择窗口");
            var regionWindow = new Views.RegionSelectionWindow();

            _logger.LogDebug("显示区域选择对话框");
            var result = regionWindow.ShowDialog();

            if (result == true && regionWindow.HasSelection)
            {
                var screenRect = regionWindow.GetScreenRegion();
                _logger.LogDebug("获取用户选择的屏幕区域: ({X:F0}, {Y:F0}) {Width:F0}x{Height:F0}",
                    screenRect.X, screenRect.Y, screenRect.Width, screenRect.Height);

                SelectedMonitoringArea = screenRect;
                HasSelectedArea = true;
                IsCustomAreaMonitoring = true;
                IsFullScreenMonitoring = false;

                MonitoringAreaInfo = $"自定义区域: ({screenRect.X:F0}, {screenRect.Y:F0}) {screenRect.Width:F0}x{screenRect.Height:F0}";

                _logger.LogInformation("区域选择成功，已设置监控区域: ({X:F0}, {Y:F0}) {Width:F0}x{Height:F0}",
                    screenRect.X, screenRect.Y, screenRect.Width, screenRect.Height);
            }
            else
            {
                _logger.LogInformation("用户取消区域选择或未选择有效区域");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "区域选择操作失败");
        }
    }



    /// <summary>
    /// 选择参考图片
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-09 16:30:00
    /// @modified: 2025-01-09 16:30:00
    /// @version: 1.2.0
    /// </summary>
    private async Task SelectReferenceImage()
    {
        try
        {
            _logger.LogInformation("[图片选择] 用户请求选择参考图片");

            // 打开文件选择对话框
            _logger.LogDebug("[图片选择] 打开文件选择对话框");
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择参考图片",
                Filter = "图像文件|*.png;*.jpg;*.jpeg;*.bmp|PNG文件|*.png|JPEG文件|*.jpg;*.jpeg|BMP文件|*.bmp|所有文件|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            var dialogResult = openFileDialog.ShowDialog();
            _logger.LogInformation("[图片选择] 文件对话框结果: {DialogResult}", dialogResult == true ? "用户选择了文件" : "用户取消选择");

            if (dialogResult == true)
            {
                var imagePath = openFileDialog.FileName;
                var fileInfo = new System.IO.FileInfo(imagePath);
                
                _logger.LogInformation("[图片选择] 用户选择文件详情 - 路径: {ImagePath}, 文件名: {FileName}, 扩展名: {Extension}, 大小: {FileSize}KB", 
                    imagePath, fileInfo.Name, fileInfo.Extension, Math.Round(fileInfo.Length / 1024.0, 2));

                // 获取详细验证结果
                _logger.LogDebug("[图片选择] 开始详细验证图像文件");
                var detailedValidationResult = await _imageService.ValidateImageFileWithDetailsAsync(imagePath);
                
                if (!detailedValidationResult.IsValid)
                {
                    var errorMessage = $"选择的图像文件无效: {detailedValidationResult.ErrorMessage}";
                    _logger.LogWarning("[图片选择] 图像文件验证失败 - 路径: {ImagePath}, 错误类型: {ErrorType}, 错误信息: {ErrorMessage}", 
                        imagePath, detailedValidationResult.ErrorType, detailedValidationResult.ErrorMessage);
                    
                    // 显示错误信息给用户
                    var messageBoxResult = System.Windows.MessageBox.Show(
                        errorMessage,
                        "图像验证失败",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    
                    _logger.LogInformation("[图片选择] 错误提示框显示完成 - 用户响应: {UserResponse}, 错误信息: {ErrorMessage}", 
                        messageBoxResult, errorMessage);
                    return;
                }

                _logger.LogInformation("[图片选择] 图像文件验证成功 - 路径: {ImagePath}, 尺寸: {Width}x{Height}", 
                    imagePath, detailedValidationResult.ImageInfo?.Width, detailedValidationResult.ImageInfo?.Height);
                
                // 更新属性
                var oldImagePath = ReferenceImagePath;
                ReferenceImagePath = imagePath;
                SelectedImagePath = imagePath;
                SelectedImageFileName = System.IO.Path.GetFileName(imagePath);
                
                _logger.LogInformation("[图片选择] 属性更新完成 - 旧路径: {OldPath}, 新路径: {NewPath}, 文件名: {FileName}", 
                    oldImagePath ?? "无", imagePath, SelectedImageFileName);

                // 加载参照图片
                await LoadReferenceImageAsync(imagePath);

                _logger.LogInformation("[图片选择] 参考图片选择流程完成 - 路径: {ImagePath}, 文件名: {FileName}", imagePath, SelectedImageFileName);
            }
            else
            {
                _logger.LogInformation("[图片选择] 用户取消了参考图片选择操作");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[图片选择] 选择参考图片过程中发生异常 - 错误信息: {ErrorMessage}", ex.Message);
            
            // 显示错误信息给用户
            var errorMessage = $"选择参考图片时发生错误：{ex.Message}";
            var messageBoxResult = System.Windows.MessageBox.Show(
                errorMessage,
                "错误",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            
            _logger.LogError("[图片选择] 异常错误提示框显示完成 - 用户响应: {UserResponse}, 异常类型: {ExceptionType}, 错误信息: {ErrorMessage}", 
                messageBoxResult, ex.GetType().Name, errorMessage);
        }
    }









    private void ClearLogs()
    {
        try
        {
            _logger.LogInformation("用户请求清空日志");

            var logCount = LogViewerViewModel?.LogEntries?.Count ?? 0;
            _logger.LogDebug("当前日志条数: {LogCount}", logCount);

            // 日志清理现在通过LogViewerViewModel处理
            LogViewerViewModel?.ClearLogs();

            _logger.LogInformation("日志清空操作完成，已清除 {LogCount} 条日志记录", logCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空日志时发生异常");
        }
    }

    // 配置管理方法已移除

    #endregion

    #region Methods

    private void InitializeCommands()
    {
        _logger.LogDebug("开始初始化UI命令");

        // 文件菜单命令
        ExitCommand = new HonyWing.UI.Commands.RelayCommand(Exit);
        _logger.LogDebug("已初始化文件菜单命令: ExitCommand");

        // 编辑菜单命令
        // OpenSettingsCommand初始化已移除

        // 工具菜单命令
        ScreenCaptureCommand = new HonyWing.UI.Commands.AsyncRelayCommand(ScreenCapture);
        AreaSelectionCommand = new HonyWing.UI.Commands.RelayCommand(AreaSelection);
        _logger.LogDebug("已初始化工具菜单命令: ScreenCaptureCommand, AreaSelectionCommand");





        // 图片选择命令
        SelectReferenceImageCommand = new HonyWing.UI.Commands.AsyncRelayCommand(SelectReferenceImage);
        _logger.LogDebug("已初始化图片选择命令: SelectReferenceImageCommand");

        // 操作控制命令
        StartMatchingCommand = new HonyWing.UI.Commands.AsyncRelayCommand(StartMatching);
        StopMatchingCommand = new HonyWing.UI.Commands.RelayCommand(StopMatching);
        PauseResumeMatchingCommand = new HonyWing.UI.Commands.RelayCommand(PauseResumeMatching);
        _logger.LogDebug("已初始化操作控制命令: StartMatchingCommand, StopMatchingCommand, PauseResumeMatchingCommand");

        // 日志管理命令
        ClearLogsCommand = new HonyWing.UI.Commands.RelayCommand(ClearLogs);
        ExportLogsCommand = new HonyWing.UI.Commands.AsyncRelayCommand(ExportLogs);
        OpenLogFolderCommand = new HonyWing.UI.Commands.RelayCommand(OpenLogFolder);
        _logger.LogDebug("已初始化日志管理命令: ClearLogsCommand, ExportLogsCommand, OpenLogFolderCommand");

        // 配置管理命令已移除

        _logger.LogInformation("UI命令初始化完成，共初始化 8 个命令");
    }

    public void Initialize()
    {
        _logger.LogInformation("开始初始化MainWindowViewModel");

        try
        {
            // 加载配置
            _logger.LogDebug("正在加载应用程序配置");
            LoadConfiguration();

            // 注册全局热键
            _logger.LogDebug("正在注册全局热键");
            RegisterGlobalHotkeys();

            _logger.LogInformation("MainWindowViewModel初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MainWindowViewModel初始化失败");
            throw;
        }
    }

    public void Cleanup()
    {
        _logger.LogInformation("开始执行MainWindowViewModel清理操作");

        // 取消订阅日志服务事件
        _logger.LogDebug("取消订阅日志服务事件");

        // 取消订阅DPI变化事件
        _logger.LogDebug("取消订阅DPI变化事件");
        _dpiAdaptationService.DpiChanged -= OnDpiChanged;

        // 停止并释放时间更新定时器
        try
        {
            _logger.LogDebug("正在释放时间更新定时器");
            _timeUpdateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timeUpdateTimer?.Dispose();
            _logger.LogDebug("时间更新定时器释放完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "释放时间更新定时器时发生异常");
        }

        // 停止匹配相关资源
        try
        {
            _logger.LogDebug("正在释放匹配相关资源");
            _matchingCancellationTokenSource?.Cancel();
            _matchingCancellationTokenSource?.Dispose();
            _matchingTimer?.Dispose();
            _matchingTimer = null;
            _logger.LogDebug("匹配相关资源释放完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "释放匹配相关资源时发生异常");
        }

        // 释放日志查看器资源
        try
        {
            _logger.LogDebug("正在释放日志查看器资源");
            _logViewerViewModel?.Dispose();
            _logger.LogDebug("日志查看器资源释放完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "释放日志查看器资源时发生异常");
        }

        // 注销全局热键
        try
        {
            _logger.LogDebug("正在注销全局热键");
            UnregisterGlobalHotkeys();
            _logger.LogDebug("全局热键注销完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注销全局热键时发生异常");
        }

        _logger.LogInformation("MainWindowViewModel清理操作完成");
    }



    private void LoadConfiguration()
    {
        try
        {
            _logger.LogInformation("开始加载应用程序配置");

            // 加载应用程序配置到属性
            _logger.LogDebug("正在加载图像匹配配置");
            TimeoutMs = _configurationService.GetValue("ImageMatching.TimeoutMs", 5000);
            SearchIntervalMs = _configurationService.GetValue("ImageMatching.SearchIntervalMs", 100);
            EnableMultiScale = _configurationService.GetValue("ImageMatching.EnableMultiScale", true);

            _logger.LogDebug("正在加载鼠标操作配置");
            ClickDelayMs = _configurationService.GetValue("Mouse.ClickDelayMs", 50);
            MoveSpeed = _configurationService.GetValue("Mouse.MoveSpeed", 2.0);
            EnableSmoothMove = _configurationService.GetValue("Mouse.EnableSmoothMove", true);
            ClickHoldDurationMs = _configurationService.GetValue("Mouse.ClickHoldDurationMs", 100);

            _logger.LogInformation("配置加载完成 - 图像匹配: {{TimeoutMs: {TimeoutMs}, SearchIntervalMs: {SearchIntervalMs}, EnableMultiScale: {EnableMultiScale}}}, 鼠标操作: {{ClickDelayMs: {ClickDelayMs}, MoveSpeed: {MoveSpeed}, EnableSmoothMove: {EnableSmoothMove}, ClickHoldDurationMs: {ClickHoldDurationMs}}}",
                TimeoutMs, SearchIntervalMs, EnableMultiScale, ClickDelayMs, MoveSpeed, EnableSmoothMove, ClickHoldDurationMs);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载配置失败");

        }
    }

    // 公共配置管理方法已移除

    // 导出和导入配置方法已移除

    /// <summary>
    /// 打开日志文件夹
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-13 16:15:00
    /// @version: 1.0.0
    /// </summary>
    private void OpenLogFolder()
    {
        try
        {
            _logger.LogInformation("用户请求打开日志文件夹");

            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HonyWing", "logs");
            _logger.LogDebug("日志文件夹路径: {LogPath}", logPath);

            if (Directory.Exists(logPath))
            {
                var logFiles = Directory.GetFiles(logPath, "*.log").Length;
                _logger.LogDebug("日志文件夹存在，包含 {LogFileCount} 个日志文件", logFiles);

                _logger.LogDebug("正在启动资源管理器打开日志文件夹");
                System.Diagnostics.Process.Start("explorer.exe", logPath);
                _logger.LogInformation("日志文件夹已成功打开: {LogPath}", logPath);
            }
            else
            {
                _logger.LogWarning("日志文件夹不存在: {LogPath}，可能尚未创建任何日志文件", logPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "打开日志文件夹时发生异常");
        }
    }



    #region DPI适配相关方法

    /// <summary>
    /// 初始化DPI信息
    /// </summary>
    private void InitializeDpiInfo()
    {
        try
        {
            _logger.LogDebug("开始初始化系统DPI信息");

            var dpiScale = _dpiAdaptationService.GetSystemDpiScale();
            SystemDpiX = 96 * dpiScale; // 标准DPI是96
            SystemDpiY = 96 * dpiScale;
            DpiScaleX = dpiScale;
            DpiScaleY = dpiScale;

            DpiInfo = _dpiAdaptationService.GetDpiInfoString();

            _logger.LogInformation("DPI信息初始化完成 - DPI缩放: {DpiScale:F2}, 系统DPI: {SystemDpiX}x{SystemDpiY}, 信息: {DpiInfo}",
                dpiScale, SystemDpiX, SystemDpiY, DpiInfo);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化DPI信息失败");

        }
    }

    /// <summary>
    /// DPI变化事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">DPI变化事件参数</param>
    private void OnDpiChanged(object? sender, DpiChangedEventArgs e)
    {
        try
        {
            // 重新获取当前DPI信息
            SystemDpiX = 96 * e.NewDpiScale; // 标准DPI是96
            SystemDpiY = 96 * e.NewDpiScale;
            DpiScaleX = e.NewDpiScale;
            DpiScaleY = e.NewDpiScale; // 假设X和Y缩放比例相同

            DpiInfo = _dpiAdaptationService.GetDpiInfoString();


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理DPI变化事件失败");

        }
    }

    #endregion



    private void UpdateCurrentTime(object? state)
    {
        CurrentTime = DateTime.Now;
    }





    /// <summary>
    /// 更新匹配统计信息
    /// </summary>
    private void UpdateMatchStatistics()
    {
        // 计算成功率
        MatchSuccessRate = TotalMatchAttempts > 0 ? (double)SuccessfulMatches / TotalMatchAttempts : 0.0;

        // 更新统计信息字符串
        var successRate = MatchSuccessRate * 100;
        MatchStatistics = $"总尝试: {TotalMatchAttempts}, 成功: {SuccessfulMatches}, 成功率: {successRate:F1}%";

        // 触发属性更改通知
        OnPropertyChanged(nameof(MatchSuccessRate));
        OnPropertyChanged(nameof(MatchStatistics));
    }

    /// <summary>
    /// 加载参照图片
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-09 16:30:00
    /// @modified: 2025-01-09 16:30:00
    /// @version: 1.2.0
    /// </summary>
    /// <param name="imagePath">图片路径</param>
    private async Task LoadReferenceImageAsync(string imagePath)
    {
        try
        {
            _logger.LogInformation("[图片加载] 开始加载参照图片: {ImagePath}", imagePath);

            // 释放之前的图片
            if (ReferenceImage != null)
            {
                _logger.LogDebug("[图片加载] 释放之前的参照图片资源 - 旧图片尺寸: {Width}x{Height}", 
                    ReferenceImage.Width, ReferenceImage.Height);
                ReferenceImage.Dispose();
                ReferenceImage = null;
            }

            // 加载新图片
            _logger.LogDebug("[图片加载] 调用图像服务加载图片");
            var loadStartTime = DateTime.Now;
            ReferenceImage = await _imageService.LoadImageAsync(imagePath);
            var loadDuration = DateTime.Now - loadStartTime;

            if (ReferenceImage == null)
            {
                _logger.LogWarning("[图片加载] 加载参照图片失败: {ImagePath}, 耗时: {Duration}ms", 
                    imagePath, loadDuration.TotalMilliseconds);
            }
            else
            {
                _logger.LogInformation("[图片加载] 参照图片加载成功: {ImagePath}, 尺寸: {Width}x{Height}, 耗时: {Duration}ms",
                    imagePath, ReferenceImage.Width, ReferenceImage.Height, loadDuration.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[图片加载] 加载参照图片异常: {ImagePath}, 异常类型: {ExceptionType}, 错误信息: {ErrorMessage}", 
                imagePath, ex.GetType().Name, ex.Message);
            ReferenceImage = null;
        }
    }

    /// <summary>
    /// 过滤日志条目
    /// </summary>
    private void FilterLogs()
    {
        // 这里可以实现日志过滤逻辑
        // 当前版本暂时不实现具体过滤，因为日志通过ObservableCollection自动更新

    }

    #region 全局热键处理

    /// <summary>
    /// 注册全局热键
    /// </summary>
    private void RegisterGlobalHotkeys()
    {
        try
        {
            // 注册Escape键（无修饰键）用于停止匹配
            _globalHotkeyService.RegisterHotkey(2, HotkeyModifiers.None, HotkeyKeys.Escape, () => OnGlobalHotkeyPressed(2));
            _logger.LogDebug("已注册全局热键: Escape键（停止匹配）");

            _logger.LogInformation("全局热键注册完成，共注册1个热键");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注册全局热键时发生异常");
        }
    }

    /// <summary>
    /// 注销全局热键
    /// </summary>
    private void UnregisterGlobalHotkeys()
    {
        try
        {
            _globalHotkeyService.UnregisterHotkey(2); // Escape键
            _logger.LogInformation("全局热键注销完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注销全局热键时发生异常");
        }
    }

    /// <summary>
    /// 全局热键按下事件处理
    /// </summary>
    private void OnGlobalHotkeyPressed(int hotkeyId)
    {
        try
        {
            _logger.LogDebug("全局热键被按下，热键ID: {HotkeyId}", hotkeyId);

            switch (hotkeyId)
            {
                case 2: // Escape键
                    if (IsRunning)
                    {
                        _logger.LogInformation("通过全局热键停止匹配，热键ID: {HotkeyId}", hotkeyId);
                        Application.Current.Dispatcher.Invoke(() => {
                            StopMatchingCommand?.Execute(null);
                        });
                    }
                    else
                    {
                        _logger.LogDebug("匹配未运行，忽略全局热键，热键ID: {HotkeyId}", hotkeyId);
                    }
                    break;
                default:
                    _logger.LogWarning("收到未知的全局热键ID: {HotkeyId}", hotkeyId);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理全局热键时发生异常，热键ID: {HotkeyId}", hotkeyId);
        }
    }

    #endregion

    #endregion
}

// UI专用的LogEntry类已移除，现在直接使用Core.Models.LogEntry

/// <summary>
/// 匹配记录
/// </summary>
public class MatchRecord
{
    public DateTime Timestamp { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public double Similarity { get; set; }
    public System.Drawing.Point? MatchLocation { get; set; }
    public System.Drawing.Point? ClickLocation { get; set; }
    public string ClickType { get; set; } = string.Empty;
    public double ExecutionTimeMs { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
