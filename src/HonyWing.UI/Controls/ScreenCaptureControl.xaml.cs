using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using HonyWing.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace HonyWing.UI.Controls;

/// <summary>
/// 屏幕截图控件
/// </summary>
public partial class ScreenCaptureControl : UserControl
{
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly IImageService _imageService;
    private readonly ILogger<ScreenCaptureControl> _logger;
    private readonly DispatcherTimer _countdownTimer;
    
    private BitmapSource? _currentCapture;
    private int _countdownSeconds;
    private string? _pendingCaptureType;

    // Windows API declarations
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public ScreenCaptureControl()
    {
        InitializeComponent();
        
        // 注意：在实际应用中，这些服务应该通过依赖注入获得
        // 这里为了演示，使用了简化的方式
        _screenCaptureService = null!; // 需要通过依赖注入设置
        _imageService = null!; // 需要通过依赖注入设置
        _logger = null!; // 需要通过依赖注入设置
        
        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _countdownTimer.Tick += OnCountdownTick;
        
        UpdateUI();
    }

    #region Constructor with Dependency Injection
    
    public ScreenCaptureControl(IScreenCaptureService screenCaptureService, 
                               IImageService imageService,
                               ILogger<ScreenCaptureControl> logger) : this()
    {
        _screenCaptureService = screenCaptureService ?? throw new ArgumentNullException(nameof(screenCaptureService));
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    #endregion

    #region Events

    /// <summary>
    /// 截图完成事件
    /// </summary>
    public event Action<BitmapSource>? CaptureCompleted;



    #endregion

    #region Event Handlers

    private async void OnFullScreenCaptureClick(object sender, RoutedEventArgs e)
    {
        await StartCaptureWithDelay("fullscreen");
    }

    private async void OnRegionCaptureClick(object sender, RoutedEventArgs e)
    {
        await StartCaptureWithDelay("region");
    }

    private async void OnWindowCaptureClick(object sender, RoutedEventArgs e)
    {
        await StartCaptureWithDelay("window");
    }



    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        if (_currentCapture == null)
            return;

        try
        {
            SetStatus("正在复制到剪贴板...");
            
            // 将BitmapSource转换为Bitmap
            var bitmap = ConvertBitmapSourceToBitmap(_currentCapture);
            if (bitmap != null)
            {
                var success = _imageService.CopyImageToClipboard(bitmap);
                if (success)
                {
                    SetStatus("截图已复制到剪贴板");
                }
                else
                {
                    SetStatus("复制到剪贴板失败");
                    // 复制失败已通过日志记录
                }
                bitmap.Dispose();
            }
            else
            {
                SetStatus("复制到剪贴板失败");
                // 图像转换失败已通过日志记录
            }
            
            _logger.LogInformation("截图已复制到剪贴板");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "复制截图到剪贴板时发生错误");
            SetStatus("复制到剪贴板失败");
            // 复制失败已通过日志记录
        }
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        ClearCapture();
    }

    private void OnPreviewImageClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && _currentCapture != null)
        {
            // 双击图像时复制到剪贴板
            OnCopyClick(sender, e);
        }
    }

    private void OnCountdownTick(object? sender, EventArgs e)
    {
        _countdownSeconds--;
        
        if (_countdownSeconds > 0)
        {
            CountdownText.Text = $"截图倒计时: {_countdownSeconds}";
        }
        else
        {
            _countdownTimer.Stop();
            CountdownText.Visibility = Visibility.Collapsed;
            
            // 执行截图
            _ = Task.Run(async () =>
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    await ExecuteCapture(_pendingCaptureType!);
                });
            });
        }
    }

    #endregion

    #region Private Methods

    private async Task StartCaptureWithDelay(string captureType)
    {
        if (_screenCaptureService == null)
        {
            // 截图服务未初始化错误已通过日志记录
            return;
        }

        var delayItem = DelayComboBox.SelectedItem as ComboBoxItem;
        var delaySeconds = int.Parse(delayItem?.Tag?.ToString() ?? "0");

        _pendingCaptureType = captureType;

        if (delaySeconds > 0)
        {
            // 开始倒计时
            _countdownSeconds = delaySeconds;
            CountdownText.Text = $"截图倒计时: {_countdownSeconds}";
            CountdownText.Visibility = Visibility.Visible;
            _countdownTimer.Start();
            
            SetStatus($"将在 {delaySeconds} 秒后开始截图...");
        }
        else
        {
            // 立即截图
            await ExecuteCapture(captureType);
        }
    }

    private async Task ExecuteCapture(string captureType)
    {
        try
        {
            SetStatus("正在截图...");
            ShowProgress(true);
            
            BitmapSource? capture = null;
            
            switch (captureType)
            {
                case "fullscreen":
                    var fullScreenBitmap = await _screenCaptureService.CaptureFullScreenAsync();
                    capture = ConvertBitmapToBitmapSource(fullScreenBitmap);
                    break;
                    
                case "region":
                    // 隐藏当前窗口以避免截图包含自身
                    var window = Window.GetWindow(this);
                    if (window != null)
                    {
                        window.WindowState = WindowState.Minimized;
                        await Task.Delay(200); // 等待窗口最小化完成
                    }
                    
                    var region = await _screenCaptureService.SelectAreaAsync();
                    if (region.HasValue)
                    {
                        var regionBitmap = await _screenCaptureService.CaptureAreaAsync(region.Value);
                        capture = ConvertBitmapToBitmapSource(regionBitmap);
                    }
                    
                    // 恢复窗口
                    if (window != null)
                    {
                        window.WindowState = WindowState.Normal;
                        window.Activate();
                    }
                    break;
                    
                case "window":
                    // 简化窗口截图，使用窗口标题
                    var inputDialog = new System.Windows.Forms.Form()
                    {
                        Text = "窗口截图",
                        Size = new System.Drawing.Size(400, 150),
                        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent,
                        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog,
                        MaximizeBox = false,
                        MinimizeBox = false
                    };
                    
                    var label = new System.Windows.Forms.Label()
                    {
                        Text = "请输入要截图的窗口标题（部分匹配）:",
                        Location = new System.Drawing.Point(10, 20),
                        Size = new System.Drawing.Size(350, 20)
                    };
                    
                    var textBox = new System.Windows.Forms.TextBox()
                    {
                        Location = new System.Drawing.Point(10, 45),
                        Size = new System.Drawing.Size(350, 25)
                    };
                    
                    var okButton = new System.Windows.Forms.Button()
                    {
                        Text = "确定",
                        Location = new System.Drawing.Point(200, 80),
                        Size = new System.Drawing.Size(75, 25),
                        DialogResult = System.Windows.Forms.DialogResult.OK
                    };
                    
                    var cancelButton = new System.Windows.Forms.Button()
                    {
                        Text = "取消",
                        Location = new System.Drawing.Point(285, 80),
                        Size = new System.Drawing.Size(75, 25),
                        DialogResult = System.Windows.Forms.DialogResult.Cancel
                    };
                    
                    inputDialog.Controls.AddRange(new System.Windows.Forms.Control[] { label, textBox, okButton, cancelButton });
                    inputDialog.AcceptButton = okButton;
                    inputDialog.CancelButton = cancelButton;
                    
                    if (inputDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        var windowHandle = FindWindowByTitle(textBox.Text);
                        if (windowHandle != IntPtr.Zero)
                        {
                            var windowBitmap = await _screenCaptureService.CaptureWindowAsync(windowHandle);
                            if (windowBitmap != null)
                            {
                                capture = ConvertBitmapToBitmapSource(windowBitmap);
                            }
                        }
                        else
                        {
                            // 窗口未找到警告已通过日志记录
                        }
                    }
                    
                    inputDialog.Dispose();
                    break;
            }
            
            if (capture != null)
            {
                SetCapture(capture);
                SetStatus("截图完成");
                CaptureCompleted?.Invoke(capture);
                
                _logger.LogInformation("截图完成，类型: {CaptureType}，尺寸: {Width}x{Height}", 
                    captureType, capture.PixelWidth, capture.PixelHeight);
            }
            else
            {
                SetStatus("截图已取消");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "截图过程中发生错误，类型: {CaptureType}", captureType);
            SetStatus("截图失败");
            // 截图失败已通过日志记录
        }
        finally
        {
            ShowProgress(false);
        }
    }

    private void SetCapture(BitmapSource capture)
    {
        _currentCapture = capture;
        PreviewImage.Source = capture;
        
        // 显示预览区域
        EmptyStatePanel.Visibility = Visibility.Collapsed;
        PreviewScrollViewer.Visibility = Visibility.Visible;
        
        // 更新图像信息
        var sizeInBytes = capture.PixelWidth * capture.PixelHeight * 4; // 假设RGBA格式
        var sizeText = FormatFileSize(sizeInBytes);
        ImageInfoText.Text = $"尺寸: {capture.PixelWidth} × {capture.PixelHeight} | 大小: {sizeText}";
        ImageInfoText.Visibility = Visibility.Visible;
        
        UpdateUI();
    }

    private void ClearCapture()
    {
        _currentCapture = null;
        PreviewImage.Source = null;
        
        // 显示空状态
        PreviewScrollViewer.Visibility = Visibility.Collapsed;
        EmptyStatePanel.Visibility = Visibility.Visible;
        
        // 隐藏图像信息
        ImageInfoText.Visibility = Visibility.Collapsed;
        
        SetStatus("就绪");
        UpdateUI();
    }

    private void UpdateUI()
    {
        var hasCapture = _currentCapture != null;
        
        CopyButton.IsEnabled = hasCapture;
        ClearButton.IsEnabled = hasCapture;
    }

    private void SetStatus(string status)
    {
        StatusText.Text = status;
    }

    private void ShowProgress(bool show)
    {
        CaptureProgressBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:n1} {suffixes[counter]}";
    }

    private static BitmapSource ConvertBitmapToBitmapSource(System.Drawing.Bitmap bitmap)
    {
        var bitmapData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            bitmap.PixelFormat);

        var bitmapSource = BitmapSource.Create(
            bitmapData.Width, bitmapData.Height,
            bitmap.HorizontalResolution, bitmap.VerticalResolution,
            System.Windows.Media.PixelFormats.Bgr32, null,
            bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

        bitmap.UnlockBits(bitmapData);
        return bitmapSource;
    }

    private static IntPtr FindWindowByTitle(string titlePart)
    {
        IntPtr foundWindow = IntPtr.Zero;
        
        EnumWindows((hWnd, lParam) =>
        {
            if (IsWindowVisible(hWnd))
            {
                int length = GetWindowTextLength(hWnd);
                if (length > 0)
                {
                    var sb = new System.Text.StringBuilder(length + 1);
                    GetWindowText(hWnd, sb, sb.Capacity);
                    string windowTitle = sb.ToString();
                    
                    if (windowTitle.Contains(titlePart, StringComparison.OrdinalIgnoreCase))
                    {
                        foundWindow = hWnd;
                        return false; // Stop enumeration
                    }
                }
            }
            return true; // Continue enumeration
        }, IntPtr.Zero);
        
        return foundWindow;
    }

    /// <summary>
    /// 将BitmapSource转换为Bitmap
    /// </summary>
    /// <param name="bitmapSource">BitmapSource对象</param>
    /// <returns>Bitmap对象</returns>
    private static Bitmap? ConvertBitmapSourceToBitmap(BitmapSource bitmapSource)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);
            memoryStream.Position = 0;
            return new Bitmap(memoryStream);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 获取当前截图
    /// </summary>
    /// <returns>当前截图，如果没有则返回null</returns>
    public BitmapSource? GetCurrentCapture()
    {
        return _currentCapture;
    }

    /// <summary>
    /// 设置截图
    /// </summary>
    /// <param name="capture">要设置的截图</param>
    public void SetCurrentCapture(BitmapSource? capture)
    {
        if (capture != null)
        {
            SetCapture(capture);
        }
        else
        {
            ClearCapture();
        }
    }

    /// <summary>
    /// 检查是否有截图
    /// </summary>
    /// <returns>如果有截图返回true，否则返回false</returns>
    public bool HasCapture => _currentCapture != null;

    #endregion
}