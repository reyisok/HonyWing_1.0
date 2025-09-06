using System.Drawing;
using System.Windows;

namespace HonyWing.Core.Interfaces;

/// <summary>
/// DPI适配服务接口
/// @author: Mr.Rey Copyright © 2025
/// @created: 2025-01-13 15:30:00
/// @version: 1.0.0
/// </summary>
public interface IDpiAdaptationService
{
    /// <summary>
    /// 获取当前系统DPI缩放比例
    /// </summary>
    /// <returns>DPI缩放比例（如1.0表示100%，1.25表示125%）</returns>
    double GetSystemDpiScale();

    /// <summary>
    /// 获取指定显示器的DPI缩放比例
    /// </summary>
    /// <param name="monitorHandle">显示器句柄</param>
    /// <returns>DPI缩放比例</returns>
    double GetMonitorDpiScale(IntPtr monitorHandle);

    /// <summary>
    /// 将设备无关像素（DIP）转换为实际像素
    /// </summary>
    /// <param name="dipPoint">设备无关像素点</param>
    /// <returns>实际像素点</returns>
    System.Drawing.Point DipToPixel(System.Drawing.Point dipPoint);

    /// <summary>
    /// 将实际像素转换为设备无关像素（DIP）
    /// </summary>
    /// <param name="pixelPoint">实际像素点</param>
    /// <returns>设备无关像素点</returns>
    System.Drawing.Point PixelToDip(System.Drawing.Point pixelPoint);

    /// <summary>
    /// 将设备无关像素矩形转换为实际像素矩形
    /// </summary>
    /// <param name="dipRect">设备无关像素矩形</param>
    /// <returns>实际像素矩形</returns>
    Rectangle DipToPixel(Rectangle dipRect);

    /// <summary>
    /// 将实际像素矩形转换为设备无关像素矩形
    /// </summary>
    /// <param name="pixelRect">实际像素矩形</param>
    /// <returns>设备无关像素矩形</returns>
    Rectangle PixelToDip(Rectangle pixelRect);

    /// <summary>
    /// 获取DPI信息字符串（用于UI显示）
    /// </summary>
    /// <returns>DPI信息字符串，如"当前DPI：125%（已自动适配）"</returns>
    string GetDpiInfoString();

    /// <summary>
    /// 检测DPI是否发生变化
    /// </summary>
    /// <returns>如果DPI发生变化返回true</returns>
    bool HasDpiChanged();

    /// <summary>
    /// 刷新DPI信息（当检测到DPI变化时调用）
    /// </summary>
    void RefreshDpiInfo();

    /// <summary>
    /// 获取指定点所在显示器的DPI缩放比例
    /// </summary>
    /// <param name="point">屏幕坐标点</param>
    /// <returns>DPI缩放比例</returns>
    double GetDpiScaleForPoint(System.Drawing.Point point);

    /// <summary>
    /// 获取指定窗口所在显示器的DPI缩放比例
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>DPI缩放比例</returns>
    double GetDpiScaleForWindow(IntPtr windowHandle);

    /// <summary>
    /// DPI变化事件
    /// </summary>
    event EventHandler<DpiChangedEventArgs>? DpiChanged;
}

/// <summary>
/// DPI变化事件参数
/// </summary>
public class DpiChangedEventArgs : EventArgs
{
    /// <summary>
    /// 旧的DPI缩放比例
    /// </summary>
    public double OldDpiScale { get; set; }

    /// <summary>
    /// 新的DPI缩放比例
    /// </summary>
    public double NewDpiScale { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="oldDpiScale">旧的DPI缩放比例</param>
    /// <param name="newDpiScale">新的DPI缩放比例</param>
    public DpiChangedEventArgs(double oldDpiScale, double newDpiScale)
    {
        OldDpiScale = oldDpiScale;
        NewDpiScale = newDpiScale;
    }
}