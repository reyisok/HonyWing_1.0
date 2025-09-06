using HonyWing.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Runtime.InteropServices;

namespace HonyWing.Infrastructure.Services;

/// <summary>
/// 鼠标操作服务实现
/// @author: Mr.Rey Copyright © 2025
/// @modified: 2025-01-17 10:45:00
/// @version: 1.1.0
/// </summary>
public class MouseService : IMouseService
{
    private readonly ILogger<MouseService> _logger;
    private readonly IClickAnimationService _clickAnimationService;
    private int _clickDelayMs = 100;

    public MouseService(ILogger<MouseService> logger, IClickAnimationService clickAnimationService)
    {
        _logger = logger;
        _clickAnimationService = clickAnimationService;
    }

    #region Windows API

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    // 鼠标事件常量
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

    #endregion

    public async Task LeftClickAsync(Point point)
    {
        try
        {
            _logger.LogDebug("执行左键单击，位置: ({X}, {Y})", point.X, point.Y);

            // 移动鼠标到目标位置
            await MoveToAsync(point);

            // 添加延迟
            if (_clickDelayMs > 0)
            {
                await Task.Delay(_clickDelayMs);
            }

            // 显示点击动画
            _ = Task.Run(async () => await _clickAnimationService.ShowClickAnimationAsync(point));

            // 执行左键按下和释放
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(10); // 短暂延迟确保按下事件被处理
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

            _logger.LogInformation("左键单击完成，位置: ({X}, {Y})", point.X, point.Y);
            
            // 自动初始化鼠标位置到屏幕左上角
            await InitializeMousePositionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "左键单击失败，位置: ({X}, {Y})", point.X, point.Y);
            throw;
        }
    }

    public async Task RightClickAsync(Point point)
    {
        try
        {
            _logger.LogDebug("执行右键单击，位置: ({X}, {Y})", point.X, point.Y);

            // 移动鼠标到目标位置
            await MoveToAsync(point);

            // 添加延迟
            if (_clickDelayMs > 0)
            {
                await Task.Delay(_clickDelayMs);
            }

            // 显示点击动画
            _ = Task.Run(async () => await _clickAnimationService.ShowClickAnimationAsync(point));

            // 执行右键按下和释放
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(10);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);

            _logger.LogInformation("右键单击完成，位置: ({X}, {Y})", point.X, point.Y);
            
            // 自动初始化鼠标位置到屏幕左上角
            await InitializeMousePositionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "右键单击失败，位置: ({X}, {Y})", point.X, point.Y);
            throw;
        }
    }

    public async Task DoubleClickAsync(Point point)
    {
        try
        {
            _logger.LogDebug("执行双击，位置: ({X}, {Y})", point.X, point.Y);

            // 移动鼠标到目标位置
            await MoveToAsync(point);

            // 添加延迟
            if (_clickDelayMs > 0)
            {
                await Task.Delay(_clickDelayMs);
            }

            // 显示点击动画
            _ = Task.Run(async () => await _clickAnimationService.ShowClickAnimationAsync(point));

            // 执行第一次点击
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(10);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

            // 双击间隔
            await Task.Delay(50);

            // 执行第二次点击
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(10);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

            _logger.LogInformation("双击完成，位置: ({X}, {Y})", point.X, point.Y);
            
            // 自动初始化鼠标位置到屏幕左上角
            await InitializeMousePositionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "双击失败，位置: ({X}, {Y})", point.X, point.Y);
            throw;
        }
    }

    public async Task LeftDoubleClickAsync(Point point)
    {
        try
        {
            _logger.LogDebug("执行左键双击，位置: ({X}, {Y})", point.X, point.Y);

            // 移动鼠标到目标位置
            await MoveToAsync(point);

            // 添加延迟
            if (_clickDelayMs > 0)
            {
                await Task.Delay(_clickDelayMs);
            }

            // 显示点击动画
            _ = Task.Run(async () => await _clickAnimationService.ShowClickAnimationAsync(point));

            // 执行第一次点击
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(10);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

            // 双击间隔
            await Task.Delay(50);

            // 执行第二次点击
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(10);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

            _logger.LogInformation("左键双击完成，位置: ({X}, {Y})", point.X, point.Y);
            
            // 自动初始化鼠标位置到屏幕左上角
            await InitializeMousePositionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "左键双击失败，位置: ({X}, {Y})", point.X, point.Y);
            throw;
        }
    }

    public async Task MiddleClickAsync(Point point)
    {
        try
        {
            _logger.LogDebug("执行中键单击，位置: ({X}, {Y})", point.X, point.Y);

            // 移动鼠标到目标位置
            await MoveToAsync(point);

            // 添加延迟
            if (_clickDelayMs > 0)
            {
                await Task.Delay(_clickDelayMs);
            }

            // 显示点击动画
            _ = Task.Run(async () => await _clickAnimationService.ShowClickAnimationAsync(point));

            // 执行中键按下和释放
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(10);
            mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, UIntPtr.Zero);

            _logger.LogInformation("中键单击完成，位置: ({X}, {Y})", point.X, point.Y);
            
            // 自动初始化鼠标位置到屏幕左上角
            await InitializeMousePositionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "中键单击失败，位置: ({X}, {Y})", point.X, point.Y);
            throw;
        }
    }

    public async Task MoveToAsync(Point point)
    {
        try
        {
            _logger.LogDebug("移动鼠标到位置: ({X}, {Y})", point.X, point.Y);

            var currentPos = GetCurrentPosition();

            // 如果已经在目标位置，直接返回
            if (currentPos.X == point.X && currentPos.Y == point.Y)
            {
                return;
            }

            // 平滑移动（可选）
            var steps = 10;
            var deltaX = (point.X - currentPos.X) / (double)steps;
            var deltaY = (point.Y - currentPos.Y) / (double)steps;

            for (int i = 1; i <= steps; i++)
            {
                var intermediateX = (int)(currentPos.X + deltaX * i);
                var intermediateY = (int)(currentPos.Y + deltaY * i);

                SetCursorPos(intermediateX, intermediateY);
                await Task.Delay(5); // 平滑移动的延迟
            }

            // 确保最终位置准确
            SetCursorPos(point.X, point.Y);

            _logger.LogDebug("鼠标移动完成，位置: ({X}, {Y})", point.X, point.Y);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "鼠标移动失败，目标位置: ({X}, {Y})", point.X, point.Y);
            throw;
        }
    }

    public async Task DragAsync(Point startPoint, Point endPoint)
    {
        try
        {
            _logger.LogDebug("执行拖拽操作，起始位置: ({StartX}, {StartY})，结束位置: ({EndX}, {EndY})",
                startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);

            // 移动到起始位置
            await MoveToAsync(startPoint);
            await Task.Delay(_clickDelayMs);

            // 按下左键
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(50);

            // 拖拽到结束位置
            var steps = 20;
            var deltaX = (endPoint.X - startPoint.X) / (double)steps;
            var deltaY = (endPoint.Y - startPoint.Y) / (double)steps;

            for (int i = 1; i <= steps; i++)
            {
                var intermediateX = (int)(startPoint.X + deltaX * i);
                var intermediateY = (int)(startPoint.Y + deltaY * i);

                SetCursorPos(intermediateX, intermediateY);
                await Task.Delay(10);
            }

            // 确保最终位置准确
            SetCursorPos(endPoint.X, endPoint.Y);
            await Task.Delay(50);

            // 释放左键
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

            _logger.LogInformation("拖拽操作完成，起始位置: ({StartX}, {StartY})，结束位置: ({EndX}, {EndY})",
                startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
            
            // 自动初始化鼠标位置到屏幕左上角
            await InitializeMousePositionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "拖拽操作失败，起始位置: ({StartX}, {StartY})，结束位置: ({EndX}, {EndY})",
                startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);

            // 确保释放鼠标按键
            try
            {
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            }
            catch
            {
                // 忽略释放按键时的异常
            }

            throw;
        }
    }

    public Point GetCurrentPosition()
    {
        try
        {
            if (GetCursorPos(out POINT point))
            {
                return new Point(point.X, point.Y);
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                _logger.LogWarning("获取鼠标位置失败，错误代码: {ErrorCode}", error);
                return Point.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取鼠标位置时发生异常");
            return Point.Empty;
        }
    }

    public void SetClickDelay(int delayMs)
    {
        if (delayMs < 0)
        {
            throw new ArgumentException("延迟时间不能为负数", nameof(delayMs));
        }

        _clickDelayMs = delayMs;
        _logger.LogDebug("点击延迟设置为: {DelayMs}ms", delayMs);
    }

    public Task InitializeMousePositionAsync()
    {
        try
        {
            _logger.LogDebug("初始化鼠标位置到屏幕左上角");
            
            // 直接移动到屏幕左上角(0,0)，不使用平滑移动以提高速度
            SetCursorPos(0, 0);
            
            _logger.LogDebug("鼠标位置初始化完成");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "鼠标位置初始化失败");
            throw;
        }
    }
}
