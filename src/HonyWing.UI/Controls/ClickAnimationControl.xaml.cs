using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HonyWing.UI.Controls
{
    /// <summary>
    /// 鼠标点击动画控件
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-17 10:30:00
    /// @version: 1.0.0
    /// </summary>
    public partial class ClickAnimationControl : UserControl
    {        
        public ClickAnimationControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 开始播放点击动画
        /// </summary>
        public void StartAnimation()
        {
            // 创建缩放动画
            var scaleAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 2.0,
                Duration = TimeSpan.FromMilliseconds(500),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(1)
            };

            // 创建透明度动画
            var opacityAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(1000)
            };

            // 获取动画元素
            var animationCircle = FindName("AnimationCircle") as FrameworkElement;
            var scaleTransform = animationCircle?.RenderTransform as ScaleTransform;
            
            if (scaleTransform != null && animationCircle != null)
            {
                // 应用动画
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                animationCircle.BeginAnimation(OpacityProperty, opacityAnimation);
            }

            // 1秒后自动隐藏控件
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                Visibility = Visibility.Collapsed;
            };
            timer.Start();
        }
    }
}