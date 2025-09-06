using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using HonyWing.Core.Interfaces;
using HonyWing.UI.Controls;
using Microsoft.Extensions.Logging;

namespace HonyWing.UI.Services
{
    /// <summary>
    /// 点击动画服务实现
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-17 10:40:00
    /// @version: 1.0.0
    /// </summary>
    public class ClickAnimationService : IClickAnimationService
    {
        private readonly ILogger<ClickAnimationService> _logger;
        private readonly IDpiAdaptationService _dpiAdaptationService;
        private Window? _animationWindow;

        public ClickAnimationService(ILogger<ClickAnimationService> logger, IDpiAdaptationService dpiAdaptationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dpiAdaptationService = dpiAdaptationService ?? throw new ArgumentNullException(nameof(dpiAdaptationService));
        }

        public async Task ShowClickAnimationAsync(System.Drawing.Point position)
        {
            try
            {
                _logger.LogDebug("开始显示点击动画，位置: ({X}, {Y})", position.X, position.Y);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // 获取当前点的DPI缩放比例
                    double dpiScale = _dpiAdaptationService.GetDpiScaleForPoint(position);
                    
                    // 将物理坐标转换为WPF逻辑坐标
                    double logicalX = position.X / dpiScale;
                    double logicalY = position.Y / dpiScale;
                    
                    _logger.LogDebug("DPI缩放比例: {DpiScale}, 物理坐标: ({PhysicalX}, {PhysicalY}), 逻辑坐标: ({LogicalX}, {LogicalY})", 
                        dpiScale, position.X, position.Y, logicalX, logicalY);

                    // 创建动画窗口（调整为更小的尺寸以匹配缩小后的动画）
                    _animationWindow = new Window
                    {
                        WindowStyle = WindowStyle.None,
                        AllowsTransparency = true,
                        Background = System.Windows.Media.Brushes.Transparent,
                        Topmost = true,
                        ShowInTaskbar = false,
                        Width = 30,  // 缩小窗口尺寸以匹配15px的圆圈
                        Height = 30, // 缩小窗口尺寸以匹配15px的圆圈
                        Left = logicalX - 15, // 居中显示（使用逻辑坐标）
                        Top = logicalY - 15,   // 居中显示（使用逻辑坐标）
                        ResizeMode = ResizeMode.NoResize,
                        IsHitTestVisible = false
                    };

                    // 创建动画控件
                    var animationControl = new ClickAnimationControl();
                    _animationWindow.Content = animationControl;

                    // 显示窗口
                    _animationWindow.Show();

                    // 开始动画
                    animationControl.StartAnimation();

                    // 1秒后关闭窗口
                    var timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(1100) // 稍微延长一点确保动画完成
                    };
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        _animationWindow?.Close();
                        _animationWindow = null;
                    };
                    timer.Start();
                });

                _logger.LogDebug("点击动画显示完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "显示点击动画时发生异常，位置: ({X}, {Y})", position.X, position.Y);
            }
        }
    }
}