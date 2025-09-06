using HonyWing.Core.Interfaces;
using HonyWing.Core.Models;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace HonyWing.Infrastructure.Services;

/// <summary>
/// 屏幕截图服务实现
/// </summary>
public class ScreenCaptureService : IScreenCaptureService
{
    private readonly ILogger<ScreenCaptureService> _logger;

    public ScreenCaptureService(ILogger<ScreenCaptureService> logger)
    {
        _logger = logger;
    }

    #region Windows API

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private const uint SRCCOPY = 0x00CC0020;

    #endregion

    public async Task<Bitmap> CaptureFullScreenAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogDebug("开始全屏截图");

                var screenBounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
                var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, screenBounds.Size, CopyPixelOperation.SourceCopy);
                }

                _logger.LogInformation("屏幕截图成功: {Width}x{Height}", screenBounds.Width, screenBounds.Height);
                return bitmap;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "屏幕截图失败");
                throw;
            }
        });
    }

    public async Task<Bitmap> CaptureRegionAsync(Rectangle region)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogDebug("开始区域截图，区域: ({X}, {Y}, {Width}, {Height})", 
                    region.X, region.Y, region.Width, region.Height);

                // 验证区域有效性
                if (region.Width <= 0 || region.Height <= 0)
                {
                    throw new ArgumentException("截图区域的宽度和高度必须大于0");
                }

                var screenBounds = GetScreenBounds();
                var clampedRegion = Rectangle.Intersect(region, screenBounds);
                
                if (clampedRegion.IsEmpty)
                {
                    throw new ArgumentException("截图区域超出屏幕范围");
                }

                var bitmap = new Bitmap(clampedRegion.Width, clampedRegion.Height, PixelFormat.Format32bppArgb);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(clampedRegion.X, clampedRegion.Y, 0, 0, 
                        clampedRegion.Size, CopyPixelOperation.SourceCopy);
                }

                _logger.LogInformation("区域截图成功: {X},{Y} {Width}x{Height}", clampedRegion.X, clampedRegion.Y, clampedRegion.Width, clampedRegion.Height);
                
                return bitmap;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "区域截图失败: {X},{Y} {Width}x{Height}", region.X, region.Y, region.Width, region.Height);
                throw;
            }
        });
    }

    public async Task<Bitmap> CaptureAreaAsync(Rectangle area)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogDebug("开始区域截图，区域: {Area}", area);

                if (area.Width <= 0 || area.Height <= 0)
                {
                    _logger.LogWarning("区域尺寸无效: {Area}", area);
                    return new Bitmap(1, 1);
                }

                var bitmap = new Bitmap(area.Width, area.Height, PixelFormat.Format32bppArgb);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    var hdcBitmap = graphics.GetHdc();
                    var hdcScreen = GetDC(IntPtr.Zero);

                    try
                    {
                        BitBlt(hdcBitmap, 0, 0, area.Width, area.Height, hdcScreen, area.X, area.Y, SRCCOPY);
                    }
                    finally
                    {
                        graphics.ReleaseHdc(hdcBitmap);
                        ReleaseDC(IntPtr.Zero, hdcScreen);
                    }
                }

                _logger.LogInformation("区域截图完成，区域: {Area}", area);
                return bitmap;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "区域截图失败，区域: {Area}", area);
                return new Bitmap(1, 1);
            }
        });
    }

    public async Task<Bitmap> CaptureWindowAsync(IntPtr windowHandle)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogDebug("开始窗口截图，窗口句柄: {WindowHandle}", windowHandle);

                if (windowHandle == IntPtr.Zero)
                {
                    _logger.LogWarning("窗口句柄无效: {WindowHandle}", windowHandle);
                    return new Bitmap(1, 1);
                }

                if (!GetWindowRect(windowHandle, out RECT rect))
                {
                    _logger.LogWarning("无法获取窗口矩形: {WindowHandle}", windowHandle);
                    return new Bitmap(1, 1);
                }

                var width = rect.Right - rect.Left;
                var height = rect.Bottom - rect.Top;

                if (width <= 0 || height <= 0)
                {
                    _logger.LogWarning("窗口尺寸无效: {WindowHandle}, 宽度: {Width}, 高度: {Height}", 
                        windowHandle, width, height);
                    return new Bitmap(1, 1);
                }

                var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    var hdcBitmap = graphics.GetHdc();
                    var hdcWindow = GetWindowDC(windowHandle);

                    try
                    {
                        BitBlt(hdcBitmap, 0, 0, width, height, hdcWindow, 0, 0, SRCCOPY);
                    }
                    finally
                    {
                        graphics.ReleaseHdc(hdcBitmap);
                        ReleaseDC(windowHandle, hdcWindow);
                    }
                }

                _logger.LogInformation("窗口截图完成，窗口: {WindowHandle}, 尺寸: {Width}x{Height}", 
                    windowHandle, width, height);
                
                return bitmap;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "窗口截图失败，窗口句柄: {WindowHandle}", windowHandle);
                throw;
            }
        });
    }

    public Size GetScreenSize()
    {
        try
        {
            var primaryScreen = Screen.PrimaryScreen;
            if (primaryScreen == null)
            {
                _logger.LogWarning("无法获取主屏幕，使用默认尺寸");
                return new Size(1920, 1080);
            }
            var size = new Size(primaryScreen.Bounds.Width, primaryScreen.Bounds.Height);
            
            _logger.LogDebug("获取屏幕尺寸: {Width}x{Height}", size.Width, size.Height);
            return size;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取屏幕尺寸失败");
            throw;
        }
    }

    public IEnumerable<ScreenInfo> GetScreens()
    {
        try
        {
            _logger.LogDebug("获取所有屏幕信息");

            var screens = new List<ScreenInfo>();
            var allScreens = Screen.AllScreens;

            for (int i = 0; i < allScreens.Length; i++)
            {
                var screen = allScreens[i];
                var screenInfo = new ScreenInfo
                {
                    Index = i,
                    IsPrimary = screen.Primary,
                    Bounds = screen.Bounds,
                    WorkingArea = screen.WorkingArea,
                    DeviceName = screen.DeviceName ?? $"Display {i + 1}"
                };
                
                screens.Add(screenInfo);
            }

            _logger.LogInformation("获取到 {Count} 个屏幕", screens.Count);
            return screens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取屏幕信息失败");
            throw;
        }
    }

    public async Task<Rectangle?> SelectAreaAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogDebug("开始区域选择");
                
                // 注意：此方法需要UI层支持，当前返回示例区域
                // 实际实现应该由UI层调用此服务
                _logger.LogWarning("SelectAreaAsync需要UI支持，返回示例区域");
                
                var screenSize = GetScreenSize();
                var sampleRegion = new Rectangle(
                    screenSize.Width / 4,
                    screenSize.Height / 4,
                    screenSize.Width / 2,
                    screenSize.Height / 2
                );
                
                return (Rectangle?)sampleRegion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "区域选择失败");
                return (Rectangle?)null;
            }
        });
    }

    private Rectangle GetScreenBounds()
    {
        var screens = Screen.AllScreens;
        if (screens.Length == 0)
        {
            return Rectangle.Empty;
        }

        var left = screens.Min(s => s.Bounds.Left);
        var top = screens.Min(s => s.Bounds.Top);
        var right = screens.Max(s => s.Bounds.Right);
        var bottom = screens.Max(s => s.Bounds.Bottom);

        return new Rectangle(left, top, right - left, bottom - top);
    }
}