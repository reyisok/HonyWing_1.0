using System.Drawing;

namespace HonyWing.Core.Interfaces;

/// <summary>
/// 鼠标操作服务接口
/// </summary>
public interface IMouseService
{
    /// <summary>
    /// 左键单击
    /// </summary>
    /// <param name="point">点击位置</param>
    Task LeftClickAsync(Point point);

    /// <summary>
    /// 右键单击
    /// </summary>
    /// <param name="point">点击位置</param>
    Task RightClickAsync(Point point);

    /// <summary>
    /// 双击
    /// </summary>
    /// <param name="point">点击位置</param>
    Task DoubleClickAsync(Point point);

    /// <summary>
    /// 左键双击
    /// </summary>
    /// <param name="point">点击位置</param>
    Task LeftDoubleClickAsync(Point point);

    /// <summary>
    /// 中键单击
    /// </summary>
    /// <param name="point">点击位置</param>
    Task MiddleClickAsync(Point point);

    /// <summary>
    /// 移动鼠标到指定位置
    /// </summary>
    /// <param name="point">目标位置</param>
    Task MoveToAsync(Point point);

    /// <summary>
    /// 拖拽操作
    /// </summary>
    /// <param name="startPoint">起始位置</param>
    /// <param name="endPoint">结束位置</param>
    Task DragAsync(Point startPoint, Point endPoint);

    /// <summary>
    /// 获取当前鼠标位置
    /// </summary>
    /// <returns>当前鼠标位置</returns>
    Point GetCurrentPosition();

    /// <summary>
    /// 设置点击延迟时间
    /// </summary>
    /// <param name="delayMs">延迟毫秒数</param>
    void SetClickDelay(int delayMs);

    /// <summary>
    /// 初始化鼠标位置到屏幕左上角
    /// </summary>
    Task InitializeMousePositionAsync();
}
