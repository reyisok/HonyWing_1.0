using System.Drawing;

namespace HonyWing.Core.Interfaces;

/// <summary>
/// 屏幕截图服务接口
/// </summary>
public interface IScreenCaptureService
{
    /// <summary>
    /// 截取全屏
    /// </summary>
    /// <returns>全屏截图</returns>
    Task<Bitmap> CaptureFullScreenAsync();

    /// <summary>
    /// 截取指定区域
    /// </summary>
    /// <param name="area">截图区域</param>
    /// <returns>区域截图</returns>
    Task<Bitmap> CaptureAreaAsync(Rectangle area);

    /// <summary>
    /// 截取指定窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>窗口截图</returns>
    Task<Bitmap> CaptureWindowAsync(IntPtr windowHandle);

    /// <summary>
    /// 获取屏幕尺寸
    /// </summary>
    /// <returns>屏幕尺寸</returns>
    Size GetScreenSize();

    /// <summary>
    /// 获取所有显示器信息
    /// </summary>
    /// <returns>显示器信息列表</returns>
    IEnumerable<ScreenInfo> GetScreens();

    /// <summary>
    /// 开始区域选择
    /// </summary>
    /// <returns>选择的区域，如果取消则返回null</returns>
    Task<Rectangle?> SelectAreaAsync();
}

/// <summary>
/// 屏幕信息
/// </summary>
public class ScreenInfo
{
    /// <summary>
    /// 屏幕索引
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 屏幕边界
    /// </summary>
    public Rectangle Bounds { get; set; }

    /// <summary>
    /// 工作区域（排除任务栏等）
    /// </summary>
    public Rectangle WorkingArea { get; set; }

    /// <summary>
    /// 是否为主屏幕
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;
}