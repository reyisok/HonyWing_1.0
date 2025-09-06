using HonyWing.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using DpiChangedEventArgs = HonyWing.Core.Interfaces.DpiChangedEventArgs;

namespace HonyWing.Infrastructure.Services;

/// <summary>
/// DPI适配服务实现
/// @author: Mr.Rey Copyright © 2025
/// @created: 2025-01-13 15:35:00
/// @version: 1.0.0
/// </summary>
public class DpiAdaptationService : IDpiAdaptationService
{
    private readonly ILogger<DpiAdaptationService> _logger;
    private double _currentDpiScale;
    private readonly object _lockObject = new();

    public DpiAdaptationService(ILogger<DpiAdaptationService> logger)
    {
        _logger = logger;
        _currentDpiScale = GetSystemDpiScale();
        _logger.LogInformation("DPI适配服务已初始化，当前DPI缩放比例: {DpiScale}%", _currentDpiScale * 100);
    }

    #region Windows API

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(System.Drawing.Point pt, uint dwFlags);

    private const int LOGPIXELSX = 88;
    private const int LOGPIXELSY = 90;
    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
    private const int MDT_EFFECTIVE_DPI = 0;

    // DPI Awareness Context values
    private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new(-4);

    #endregion

    public event EventHandler<DpiChangedEventArgs>? DpiChanged;

    public double GetSystemDpiScale()
    {
        try
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero)
            {
                _logger.LogWarning("无法获取设备上下文，使用默认DPI缩放比例1.0");
                return 1.0;
            }

            try
            {
                int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
                double scale = dpiX / 96.0; // 96 DPI是100%缩放
                _logger.LogDebug("检测到系统DPI: {Dpi}，缩放比例: {Scale}", dpiX, scale);
                return scale;
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取系统DPI缩放比例时发生错误");
            return 1.0;
        }
    }

    public double GetMonitorDpiScale(IntPtr monitorHandle)
    {
        try
        {
            int result = GetDpiForMonitor(monitorHandle, MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
            if (result == 0) // S_OK
            {
                double scale = dpiX / 96.0;
                _logger.LogDebug("显示器DPI: {DpiX}x{DpiY}，缩放比例: {Scale}", dpiX, dpiY, scale);
                return scale;
            }
            else
            {
                _logger.LogWarning("获取显示器DPI失败，错误代码: {ErrorCode}，使用系统DPI", result);
                return GetSystemDpiScale();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取显示器DPI缩放比例时发生错误");
            return GetSystemDpiScale();
        }
    }

    public System.Drawing.Point DipToPixel(System.Drawing.Point dipPoint)
    {
        lock (_lockObject)
        {
            int pixelX = (int)Math.Round(dipPoint.X * _currentDpiScale);
            int pixelY = (int)Math.Round(dipPoint.Y * _currentDpiScale);
            return new System.Drawing.Point(pixelX, pixelY);
        }
    }

    public System.Drawing.Point PixelToDip(System.Drawing.Point pixelPoint)
    {
        lock (_lockObject)
        {
            int dipX = (int)Math.Round(pixelPoint.X / _currentDpiScale);
            int dipY = (int)Math.Round(pixelPoint.Y / _currentDpiScale);
            return new System.Drawing.Point(dipX, dipY);
        }
    }

    public Rectangle DipToPixel(Rectangle dipRect)
    {
        lock (_lockObject)
        {
            int x = (int)Math.Round(dipRect.X * _currentDpiScale);
            int y = (int)Math.Round(dipRect.Y * _currentDpiScale);
            int width = (int)Math.Round(dipRect.Width * _currentDpiScale);
            int height = (int)Math.Round(dipRect.Height * _currentDpiScale);
            return new Rectangle(x, y, width, height);
        }
    }

    public Rectangle PixelToDip(Rectangle pixelRect)
    {
        lock (_lockObject)
        {
            int x = (int)Math.Round(pixelRect.X / _currentDpiScale);
            int y = (int)Math.Round(pixelRect.Y / _currentDpiScale);
            int width = (int)Math.Round(pixelRect.Width / _currentDpiScale);
            int height = (int)Math.Round(pixelRect.Height / _currentDpiScale);
            return new Rectangle(x, y, width, height);
        }
    }

    public string GetDpiInfoString()
    {
        lock (_lockObject)
        {
            int percentage = (int)Math.Round(_currentDpiScale * 100);
            return $"当前DPI：{percentage}%（已自动适配）";
        }
    }

    public bool HasDpiChanged()
    {
        double newDpiScale = GetSystemDpiScale();
        lock (_lockObject)
        {
            return Math.Abs(newDpiScale - _currentDpiScale) > 0.01; // 允许1%的误差
        }
    }

    public void RefreshDpiInfo()
    {
        double oldDpiScale;
        double newDpiScale = GetSystemDpiScale();
        
        lock (_lockObject)
        {
            oldDpiScale = _currentDpiScale;
            if (Math.Abs(newDpiScale - _currentDpiScale) > 0.01)
            {
                _currentDpiScale = newDpiScale;
                _logger.LogInformation("DPI缩放比例已更新: {OldScale}% -> {NewScale}%", 
                    oldDpiScale * 100, newDpiScale * 100);
            }
            else
            {
                return; // 没有变化，不触发事件
            }
        }

        // 在锁外触发事件，避免死锁
        try
        {
            DpiChanged?.Invoke(this, new DpiChangedEventArgs(oldDpiScale, newDpiScale));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "触发DPI变化事件时发生错误");
        }
    }

    /// <summary>
    /// 设置应用程序DPI感知模式
    /// 应在应用程序启动时调用
    /// </summary>
    /// <returns>是否设置成功</returns>
    public static bool SetDpiAwareness()
    {
        try
        {
            // 设置为Per-Monitor DPI Aware V2模式
            bool result = SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            return result;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 获取指定点所在显示器的DPI缩放比例
    /// </summary>
    /// <param name="point">屏幕坐标点</param>
    /// <returns>DPI缩放比例</returns>
    public double GetDpiScaleForPoint(System.Drawing.Point point)
    {
        try
        {
            IntPtr monitor = MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST);
            return GetMonitorDpiScale(monitor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取指定点DPI缩放比例时发生错误");
            return _currentDpiScale;
        }
    }

    /// <summary>
    /// 获取指定窗口所在显示器的DPI缩放比例
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>DPI缩放比例</returns>
    public double GetDpiScaleForWindow(IntPtr windowHandle)
    {
        try
        {
            IntPtr monitor = MonitorFromWindow(windowHandle, MONITOR_DEFAULTTONEAREST);
            return GetMonitorDpiScale(monitor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取指定窗口的DPI缩放比例时发生错误");
            return _currentDpiScale;
        }
    }
}