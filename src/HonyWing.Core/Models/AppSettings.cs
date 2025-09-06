using System.Drawing;

namespace HonyWing.Core.Models;

/// <summary>
/// 应用程序设置
/// @author: Mr.Rey Copyright © 2025
/// @modified: 2025-01-05 20:35:00
/// @version: 1.0.1
/// </summary>
public class AppSettings
{
    /// <summary>
    /// 通用设置
    /// </summary>
    public GeneralSettings General { get; set; } = new();

    /// <summary>
    /// 图像匹配设置
    /// </summary>
    public ImageMatchingSettings ImageMatching { get; set; } = new();

    /// <summary>
    /// 鼠标操作设置
    /// </summary>
    public MouseSettings Mouse { get; set; } = new();

    /// <summary>
    /// 界面设置
    /// </summary>
    public UISettings UI { get; set; } = new();

    /// <summary>
    /// 日志设置
    /// </summary>
    public LoggingSettings Logging { get; set; } = new();

    #region 扁平化属性 - 用于UI绑定



    /// <summary>
    /// 匹配阈值
    /// </summary>
    public double MatchThreshold
    {
        get => ImageMatching.DefaultThreshold;
        set => ImageMatching.DefaultThreshold = value;
    }

    /// <summary>
    /// 超时时间
    /// </summary>
    public int TimeoutMs
    {
        get => ImageMatching.TimeoutMs;
        set => ImageMatching.TimeoutMs = value;
    }

    /// <summary>
    /// 搜索间隔
    /// </summary>
    public int SearchIntervalMs
    {
        get => ImageMatching.SearchIntervalMs;
        set => ImageMatching.SearchIntervalMs = value;
    }

    /// <summary>
    /// 启用多尺度匹配
    /// </summary>
    public bool EnableMultiScale
    {
        get => ImageMatching.EnableMultiScale;
        set => ImageMatching.EnableMultiScale = value;
    }

    /// <summary>
    /// 点击延迟
    /// </summary>
    public int ClickDelayMs
    {
        get => Mouse.ClickDelayMs;
        set => Mouse.ClickDelayMs = value;
    }

    /// <summary>
    /// 移动速度
    /// </summary>
    public double MoveSpeed
    {
        get => Mouse.MoveSpeed;
        set => Mouse.MoveSpeed = value;
    }

    /// <summary>
    /// 启用平滑移动
    /// </summary>
    public bool EnableSmoothMove
    {
        get => Mouse.EnableSmoothMove;
        set => Mouse.EnableSmoothMove = value;
    }

    /// <summary>
    /// 点击停留时长
    /// </summary>
    public int ClickHoldDurationMs
    {
        get => Mouse.ClickHoldDurationMs;
        set => Mouse.ClickHoldDurationMs = value;
    }

    /// <summary>
    /// 多目标点击间隔
    /// </summary>
    public int MultiTargetIntervalMs
    {
        get => Mouse.MultiTargetIntervalMs;
        set => Mouse.MultiTargetIntervalMs = value;
    }

    /// <summary>
    /// 日志级别
    /// </summary>
    public string LogLevel
    {
        get => Logging.LogLevel;
        set => Logging.LogLevel = value;
    }

    /// <summary>
    /// 启用文件日志
    /// </summary>
    public bool EnableFileLogging
    {
        get => Logging.EnableFileLogging;
        set => Logging.EnableFileLogging = value;
    }

    /// <summary>
    /// 最大文件大小
    /// </summary>
    public int MaxFileSizeMB
    {
        get => Logging.MaxFileSizeMB;
        set => Logging.MaxFileSizeMB = value;
    }

    #endregion

    /// <summary>
    /// 克隆设置对象
    /// </summary>
    /// <returns>克隆的设置对象</returns>
    public AppSettings Clone()
    {
        return new AppSettings
        {
            General = new GeneralSettings
            {
                CheckForUpdates = General.CheckForUpdates
            },
            ImageMatching = new ImageMatchingSettings
            {
                DefaultThreshold = ImageMatching.DefaultThreshold,
                TimeoutMs = ImageMatching.TimeoutMs,
                SearchIntervalMs = ImageMatching.SearchIntervalMs,
                MaxSearchAttempts = ImageMatching.MaxSearchAttempts,
                EnableMultiScale = ImageMatching.EnableMultiScale,
                ScaleRange = (double[])ImageMatching.ScaleRange.Clone()
            },
            Mouse = new MouseSettings
            {
                ClickDelayMs = Mouse.ClickDelayMs,
                MoveSpeed = Mouse.MoveSpeed,
                EnableSmoothMove = Mouse.EnableSmoothMove,
                DoubleClickIntervalMs = Mouse.DoubleClickIntervalMs,
                DragDelayMs = Mouse.DragDelayMs,
                ClickHoldDurationMs = Mouse.ClickHoldDurationMs,
                MultiTargetIntervalMs = Mouse.MultiTargetIntervalMs
            },
            UI = new UISettings
            {
        
                AccentColor = UI.AccentColor,
                WindowLocation = UI.WindowLocation,
                WindowSize = UI.WindowSize,
                WindowState = UI.WindowState,
                ShowGridLines = UI.ShowGridLines,
                ShowCoordinates = UI.ShowCoordinates
            },
            Logging = new LoggingSettings
            {
                LogLevel = Logging.LogLevel,
                EnableFileLogging = Logging.EnableFileLogging,
                EnableConsoleLogging = Logging.EnableConsoleLogging,
                MaxFileSizeMB = Logging.MaxFileSizeMB,
                RetainedFileCount = Logging.RetainedFileCount,
                OutputTemplate = Logging.OutputTemplate
            }
        };
    }
}

/// <summary>
/// 通用设置
/// </summary>
public class GeneralSettings
{
    /// <summary>
    /// 检查更新
    /// </summary>
    public bool CheckForUpdates { get; set; } = true;
}

/// <summary>
/// 图像匹配设置
/// </summary>
public class ImageMatchingSettings
{
    /// <summary>
    /// 默认匹配阈值
    /// </summary>
    public double DefaultThreshold { get; set; } = 0.8;

    /// <summary>
    /// 匹配超时时间（毫秒）
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;

    /// <summary>
    /// 搜索间隔（毫秒）
    /// </summary>
    public int SearchIntervalMs { get; set; } = 100;

    /// <summary>
    /// 最大搜索次数
    /// </summary>
    public int MaxSearchAttempts { get; set; } = 50;

    /// <summary>
    /// 启用多尺度匹配
    /// </summary>
    public bool EnableMultiScale { get; set; } = true;

    /// <summary>
    /// 缩放范围
    /// </summary>
    public double[] ScaleRange { get; set; } = { 0.8, 1.2 };
}

/// <summary>
/// 鼠标操作设置
/// </summary>
public class MouseSettings
{
    /// <summary>
    /// 点击延迟（毫秒）
    /// </summary>
    public int ClickDelayMs { get; set; } = 100;

    /// <summary>
    /// 移动速度（像素/毫秒）
    /// </summary>
    public double MoveSpeed { get; set; } = 2.0;

    /// <summary>
    /// 启用平滑移动
    /// </summary>
    public bool EnableSmoothMove { get; set; } = true;

    /// <summary>
    /// 双击间隔（毫秒）
    /// </summary>
    public int DoubleClickIntervalMs { get; set; } = 200;

    /// <summary>
    /// 拖拽延迟（毫秒）
    /// </summary>
    public int DragDelayMs { get; set; } = 50;

    /// <summary>
    /// 点击停留时长（毫秒）
    /// </summary>
    public int ClickHoldDurationMs { get; set; } = 200;

    /// <summary>
    /// 多目标点击间隔（毫秒）
    /// </summary>
    public int MultiTargetIntervalMs { get; set; } = 500;
}

/// <summary>
/// 界面设置
/// </summary>
public class UISettings
{


    /// <summary>
    /// 主色调
    /// </summary>
    public string AccentColor { get; set; } = "#0078D4";

    /// <summary>
    /// 窗口位置
    /// </summary>
    public Point WindowLocation { get; set; } = new(100, 100);

    /// <summary>
    /// 窗口大小
    /// </summary>
    public Size WindowSize { get; set; } = new(1200, 800);

    /// <summary>
    /// 窗口状态
    /// </summary>
    public string WindowState { get; set; } = "Normal";

    /// <summary>
    /// 显示网格线
    /// </summary>
    public bool ShowGridLines { get; set; } = true;

    /// <summary>
    /// 显示坐标信息
    /// </summary>
    public bool ShowCoordinates { get; set; } = true;
}

/// <summary>
/// 日志设置
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public string LogLevel { get; set; } = "Information";

    /// <summary>
    /// 启用文件日志
    /// </summary>
    public bool EnableFileLogging { get; set; } = true;

    /// <summary>
    /// 启用控制台日志
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// 日志文件最大大小（MB）
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 10;

    /// <summary>
    /// 保留日志文件数量
    /// </summary>
    public int RetainedFileCount { get; set; } = 30;

    /// <summary>
    /// 日志输出格式
    /// </summary>
    public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
}