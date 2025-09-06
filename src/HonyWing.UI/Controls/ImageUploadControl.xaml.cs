using HonyWing.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HonyWing.UI.Controls;

/// <summary>
/// 图像上传控件
/// </summary>
public partial class ImageUploadControl : UserControl
{
    private readonly IImageService _imageService;
    private readonly IImageTemplateService _templateService;
    private readonly ILogger<ImageUploadControl> _logger;
    private readonly INotificationService _notificationService;
    private string? _currentImagePath;
    
    /// <summary>
    /// 当前图像的自定义名称
    /// </summary>
    private string? _customImageName;
    
    /// <summary>
    /// 当前图像的备注信息
    /// </summary>
    private string? _imageComment;
    
    private Bitmap? _currentImage;
    private bool _isDragOver;

    public ImageUploadControl()
    {
        InitializeComponent();
        
        // 从服务容器获取依赖
        var serviceProvider = App.ServiceProvider!;
        _imageService = serviceProvider.GetRequiredService<IImageService>();
        _templateService = serviceProvider.GetRequiredService<IImageTemplateService>();
        _logger = serviceProvider.GetRequiredService<ILogger<ImageUploadControl>>();
        _notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        // 订阅Unloaded事件
        this.Unloaded += OnUnloaded;
        
        // 更新支持格式显示
        UpdateSupportedFormatsText();
    }

    #region Public Properties

    /// <summary>
    /// 当前加载的图像
    /// </summary>
    public Bitmap? CurrentImage => _currentImage;

    /// <summary>
    /// 当前图像文件路径
    /// </summary>
    public string? CurrentImagePath => _currentImagePath;

    /// <summary>
    /// 是否有图像加载
    /// </summary>
    public bool HasImage => _currentImage != null;

    #endregion

    #region Public Events

    /// <summary>
    /// 图像加载事件
    /// </summary>
    public event EventHandler<Bitmap>? ImageLoaded;

    /// <summary>
    /// 图像清空事件
    /// </summary>
    public event EventHandler? ImageCleared;



    #endregion

    #region Drag and Drop Events

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1 && IsImageFile(files[0]))
            {
                e.Effects = DragDropEffects.Copy;
                SetDragOverState(true);
                return;
            }
        }
        
        e.Effects = DragDropEffects.None;
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        SetDragOverState(false);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1 && IsImageFile(files[0]))
            {
                e.Effects = DragDropEffects.Copy;
                return;
            }
        }
        
        e.Effects = DragDropEffects.None;
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        SetDragOverState(false);
        
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1 && IsImageFile(files[0]))
            {
                await LoadImageAsync(files[0]);
            }
        }
    }

    #endregion

    #region Button Events

    private async void OnBrowseClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择图像文件",
                Filter = GetImageFilter(),
                FilterIndex = 1,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await LoadImageAsync(openFileDialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "浏览文件失败");
            _notificationService.ShowError($"浏览文件失败：{ex.Message}");
        }
    }

    private async void OnPasteClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var clipboardImage = _imageService.GetImageFromClipboard();
            if (clipboardImage != null)
            {
                await LoadImageFromBitmapAsync(clipboardImage, "剪贴板图像");
            }
            else
            {
                _notificationService.ShowInfo("剪贴板中没有图像数据", "提示");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从剪贴板粘贴失败");
            _notificationService.ShowError($"从剪贴板粘贴失败：{ex.Message}");
        }
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        ShowDeleteConfirmationDialog();
    }

    /// <summary>
    /// 显示删除确认对话框
    /// </summary>
    private void ShowDeleteConfirmationDialog()
    {
        try
        {
            // 删除确认对话框暂时禁用，默认不删除
            var result = MessageBoxResult.No;

            if (result == MessageBoxResult.Yes)
            {
                ClearImage();
                _logger?.LogInformation("User confirmed deletion of reference image");
            }
            else
            {
                _logger?.LogInformation("User cancelled deletion of reference image");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while showing delete confirmation dialog");
            // 如果对话框出现异常，直接执行删除操作
            ClearImage();
        }
    }




    private void OnCopyToClipboardClick(object sender, RoutedEventArgs e)
    {
        if (_currentImage == null)
        {
            return;
        }

        try
        {
            if (_imageService.CopyImageToClipboard(_currentImage))
            {
                // 复制成功消息已通过日志记录
            }
            else
            {
                // 复制失败已通过日志记录
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "复制到剪贴板失败");
            // 复制失败已通过日志记录
        }
    }

    private async void OnSaveAsClick(object sender, RoutedEventArgs e)
    {
        if (_currentImage == null)
        {
            return;
        }

        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "保存图像",
                Filter = GetImageFilter(),
                FilterIndex = 1,
                FileName = Path.GetFileNameWithoutExtension(_currentImagePath ?? "image") + ".png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var format = GetImageFormatFromExtension(Path.GetExtension(saveFileDialog.FileName));
                if (await _imageService.SaveImageAsync(_currentImage, saveFileDialog.FileName, format))
                {
                    // 保存成功消息已通过日志记录
                }
                else
                {
                    // 保存失败已通过日志记录
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存图像失败");
            // 保存失败已通过日志记录
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 加载图像文件
    /// </summary>
    /// <param name="filePath">图像文件路径</param>
    public async Task<bool> LoadImageAsync(string filePath)
    {
        try
        {
            _logger.LogDebug("加载图像文件: {FilePath}", filePath);

            // 使用详细验证方法
            var validationResult = await _imageService.ValidateImageFileWithDetailsAsync(filePath);
            if (!validationResult.IsValid)
            {
                // 显示详细的错误提示弹窗
                ShowValidationErrorDialog(validationResult);
                return false;
            }

            var image = await _imageService.LoadImageAsync(filePath);
            if (image == null)
            {
                // 加载失败已通过日志记录
                return false;
            }

            // 清理之前的图像
            _currentImage?.Dispose();

            _currentImage = image;
            _currentImagePath = filePath;

            // 更新UI
            await UpdatePreviewAsync();
            await UpdateImageInfoAsync();
            UpdateUIState();

            // 初始化缩放状态
            SetImageZoom(1.0);

            // 触发事件
            ImageLoaded?.Invoke(this, image);

            _logger.LogInformation("成功加载图像: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载图像失败: {FilePath}", filePath);
            // 加载失败已通过日志记录
            return false;
        }
    }

    /// <summary>
    /// 从Bitmap加载图像
    /// </summary>
    /// <param name="bitmap">图像对象</param>
    /// <param name="displayName">显示名称</param>
    public async Task<bool> LoadImageFromBitmapAsync(Bitmap bitmap, string displayName = "未知图像")
    {
        try
        {
            _logger.LogDebug("从位图加载图像: {DisplayName}", displayName);

            // 清理之前的图像
            _currentImage?.Dispose();

            _currentImage = new Bitmap(bitmap); // 创建副本
            _currentImagePath = null; // 没有文件路径

            // 更新UI
            await UpdatePreviewAsync();
            UpdateImageInfoForBitmap(displayName);
            UpdateUIState();

            // 初始化缩放状态
            SetImageZoom(1.0);

            // 触发事件
            ImageLoaded?.Invoke(this, _currentImage);

            _logger.LogInformation("成功从位图加载图像: {DisplayName}", displayName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从位图加载图像失败: {DisplayName}", displayName);
            // 加载失败已通过日志记录
            return false;
        }
    }

    /// <summary>
    /// 清空当前图像
    /// </summary>
    public void ClearImage()
    {
        try
        {
            _logger.LogDebug("清除当前图像");

            // 清理资源
        _currentImage?.Dispose();
        _currentImage = null;
        _currentImagePath = null;
        _customImageName = null;
        _imageComment = null;
        
        // 更新UI状态
        UpdateUIState();
        
        // 触发事件
        ImageCleared?.Invoke(this, EventArgs.Empty);

            _logger.LogDebug("图像已清除");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除图像失败");
        }
    }

    #endregion

    #region Private Methods

    private void UpdateSupportedFormatsText()
    {
        var formats = _imageService.GetSupportedFormats();
        var formatsText = string.Join(", ", formats.Select(f => f.TrimStart('.').ToUpperInvariant()));
        SupportedFormatsText.Text = $"支持格式：{formatsText}";
    }

    private bool IsImageFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var supportedFormats = _imageService.GetSupportedFormats();
        return supportedFormats.Contains(extension);
    }

    private string GetImageFilter()
    {
        var formats = _imageService.GetSupportedFormats();
        var filterParts = new List<string>();
        
        // 添加"所有支持的图像"过滤器
        var allExtensions = string.Join(";", formats.Select(f => $"*{f}"));
        filterParts.Add($"所有支持的图像|{allExtensions}");
        
        // 添加具体格式过滤器
        var formatNames = new Dictionary<string, string>
        {
            {".png", "PNG 图像"},
            {".jpg", "JPEG 图像"},
            {".jpeg", "JPEG 图像"},
            {".bmp", "位图图像"},
            {".gif", "GIF 图像"},
            {".tiff", "TIFF 图像"},
            {".tif", "TIFF 图像"}
        };
        
        foreach (var format in formats)
        {
            if (formatNames.TryGetValue(format, out var name))
            {
                filterParts.Add($"{name}|*{format}");
            }
        }
        
        filterParts.Add("所有文件|*.*");
        
        return string.Join("|", filterParts);
    }

    private Core.Interfaces.ImageFormat GetImageFormatFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".png" => Core.Interfaces.ImageFormat.Png,
            ".jpg" or ".jpeg" => Core.Interfaces.ImageFormat.Jpeg,
            ".bmp" => Core.Interfaces.ImageFormat.Bmp,
            ".gif" => Core.Interfaces.ImageFormat.Gif,
            ".tiff" or ".tif" => Core.Interfaces.ImageFormat.Tiff,
            _ => Core.Interfaces.ImageFormat.Png
        };
    }

    private void SetDragOverState(bool isDragOver)
    {
        _isDragOver = isDragOver;
        
        if (isDragOver)
        {
            UploadArea.Style = (Style)FindResource("DragOverStyle");
        }
        else
        {
            UploadArea.Style = (Style)FindResource("UploadAreaStyle");
        }
    }

    private async Task UpdatePreviewAsync()
    {
        if (_currentImage == null)
        {
            return;
        }

        try
        {
            // 转换为WPF可显示的格式
            var bitmapSource = await Task.Run(() =>
            {
                using var memory = new MemoryStream();
                _currentImage.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                
                return bitmapImage;
            });

            PreviewImage.Source = bitmapSource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新图像预览失败");
        }
    }

    private async Task UpdateImageInfoAsync()
    {
        if (string.IsNullOrEmpty(_currentImagePath))
        {
            return;
        }

        try
        {
            var imageInfo = await _imageService.GetImageInfoAsync(_currentImagePath);
            if (imageInfo != null)
            {
                // 更新可编辑的文件名
            FileNameEditBox.Text = _customImageName ?? Path.GetFileNameWithoutExtension(imageInfo.FileName);
            
            FileSizeText.Text = FormatFileSize(imageInfo.FileSize);
            ImageSizeText.Text = $"{imageInfo.Width} × {imageInfo.Height}";
            ImageFormatText.Text = Path.GetExtension(_currentImagePath).TrimStart('.').ToUpperInvariant();
            ResolutionText.Text = $"{imageInfo.HorizontalResolution:F0} × {imageInfo.VerticalResolution:F0} DPI";
            ColorDepthText.Text = GetColorDepthDescription(imageInfo.PixelFormat);
            CreationTimeText.Text = imageInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
            FilePathText.Text = imageInfo.FilePath;
            
            // 更新备注信息
            CommentTextBox.Text = _imageComment ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新图像信息失败");
        }
    }

    private void UpdateImageInfoForBitmap(string displayName)
    {
        if (_currentImage == null)
        {
            return;
        }

        try
        {
            // 更新可编辑的文件名
        FileNameEditBox.Text = _customImageName ?? displayName;
        
        FileSizeText.Text = "未知";
        ImageSizeText.Text = $"{_currentImage.Width} × {_currentImage.Height}";
        ImageFormatText.Text = _currentImage.PixelFormat.ToString();
        ResolutionText.Text = $"{_currentImage.HorizontalResolution:F0} × {_currentImage.VerticalResolution:F0} DPI";
        ColorDepthText.Text = GetColorDepthDescription(_currentImage.PixelFormat.ToString());
        CreationTimeText.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        FilePathText.Text = "内存中的图像";
        
        // 更新备注信息
        CommentTextBox.Text = _imageComment ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新位图图像信息失败");
        }
    }

    private void UpdateUIState()
    {
        var hasImage = _currentImage != null;
        
        UploadArea.Visibility = hasImage ? Visibility.Collapsed : Visibility.Visible;
        PreviewArea.Visibility = hasImage ? Visibility.Visible : Visibility.Collapsed;
        ActionButtons.Visibility = hasImage ? Visibility.Visible : Visibility.Collapsed;
        ClearButton.IsEnabled = hasImage;
    }

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:n1} {suffixes[counter]}";
    }

    /// <summary>
    /// 获取色彩深度描述
    /// </summary>
    /// <param name="pixelFormat">像素格式</param>
    /// <returns>色彩深度描述</returns>
    private static string GetColorDepthDescription(string pixelFormat)
    {
        return pixelFormat.ToLowerInvariant() switch
        {
            var format when format.Contains("1bpp") => "1位 (单色)",
            var format when format.Contains("4bpp") => "4位 (16色)",
            var format when format.Contains("8bpp") => "8位 (256色)",
            var format when format.Contains("16bpp") => "16位 (65,536色)",
            var format when format.Contains("24bpp") => "24位 (1,677万色)",
            var format when format.Contains("32bpp") => "32位 (1,677万色 + Alpha)",
            var format when format.Contains("48bpp") => "48位 (高色深)",
            var format when format.Contains("64bpp") => "64位 (高色深 + Alpha)",
            var format when format.Contains("indexed") => "索引色",
            var format when format.Contains("grayscale") => "灰度",
            var format when format.Contains("rgb") => "RGB彩色",
            var format when format.Contains("argb") => "ARGB彩色 (带透明度)",
            _ => pixelFormat
        };
    }

    /// <summary>
    /// 缩小按钮点击事件
    /// </summary>
    private void OnZoomOutClick(object sender, RoutedEventArgs e)
    {
        SetImageZoom(0.5);
    }

    /// <summary>
    /// 原始尺寸按钮点击事件
    /// </summary>
    private void OnZoomOriginalClick(object sender, RoutedEventArgs e)
    {
        SetImageZoom(1.0);
    }

    /// <summary>
    /// 放大按钮点击事件
    /// </summary>
    private void OnZoomInClick(object sender, RoutedEventArgs e)
    {
        SetImageZoom(2.0);
    }

    /// <summary>
    /// 设置图像缩放比例
    /// </summary>
    /// <param name="scale">缩放比例</param>
    private void SetImageZoom(double scale)
    {
        try
        {
            if (PreviewImage?.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.ScaleX = scale;
                scaleTransform.ScaleY = scale;
                
                // 更新按钮状态
                UpdateZoomButtonStates(scale);
                
                _logger?.LogInformation($"Image zoom scale set to: {scale:P0}");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while setting image zoom scale");
        }
    }

    /// <summary>
    /// 更新缩放按钮状态
    /// </summary>
    /// <param name="currentScale">当前缩放比例</param>
    private void UpdateZoomButtonStates(double currentScale)
    {
        try
        {
            // 重置所有按钮样式
            ZoomOutButton.Style = (Style)FindResource("SecondaryButtonStyle");
            ZoomOriginalButton.Style = (Style)FindResource("SecondaryButtonStyle");
            ZoomInButton.Style = (Style)FindResource("SecondaryButtonStyle");
            
            // 高亮当前激活的按钮
            if (Math.Abs(currentScale - 0.5) < 0.01)
            {
                ZoomOutButton.Style = (Style)FindResource("PrimaryButtonStyle");
            }
            else if (Math.Abs(currentScale - 1.0) < 0.01)
            {
                ZoomOriginalButton.Style = (Style)FindResource("PrimaryButtonStyle");
            }
            else if (Math.Abs(currentScale - 2.0) < 0.01)
             {
                 ZoomInButton.Style = (Style)FindResource("PrimaryButtonStyle");
             }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while updating zoom button states");
        }
    }

    /// <summary>
    /// 重命名按钮点击事件
    /// </summary>
    private void OnRenameClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var newName = FileNameEditBox.Text?.Trim();
            if (string.IsNullOrEmpty(newName))
            {
                // 文件名验证失败已通过日志记录
                return;
            }
            
            // 验证文件名合法性
            if (ContainsInvalidFileNameChars(newName))
            {
                // 文件名验证失败已通过日志记录
                return;
            }
            
            _customImageName = newName;
            _logger?.LogInformation($"Image renamed to: {newName}");
            
            // 重命名成功已通过日志记录
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while renaming image");
            // 重命名失败已通过日志记录
        }
    }
    
    /// <summary>
    /// 文件名输入框失去焦点事件
    /// </summary>
    private void OnFileNameLostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            var newName = FileNameEditBox.Text?.Trim();
            if (!string.IsNullOrEmpty(newName) && newName != _customImageName)
            {
                if (!ContainsInvalidFileNameChars(newName))
                {
                    _customImageName = newName;
                    _logger?.LogInformation($"Image name automatically updated to: {newName}");
                }
                else
                {
                    // 恢复原来的名称
                    FileNameEditBox.Text = _customImageName ?? Path.GetFileNameWithoutExtension(_currentImagePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while handling filename lost focus event");
        }
    }
    
    /// <summary>
    /// 备注输入框失去焦点事件
    /// </summary>
    private void OnCommentLostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            _imageComment = CommentTextBox.Text?.Trim();
            _logger?.LogInformation($"Image comment updated, length: {_imageComment?.Length ?? 0} characters");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while handling comment lost focus event");
        }
    }
    
    /// <summary>
    /// 检查文件名是否包含非法字符
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>是否包含非法字符</returns>
    private static bool ContainsInvalidFileNameChars(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return fileName.Any(c => invalidChars.Contains(c));
    }
    
    /// <summary>
    /// 获取当前图像的显示名称
    /// </summary>
    /// <returns>显示名称</returns>
    public string GetCurrentImageDisplayName()
    {
        return _customImageName ?? Path.GetFileNameWithoutExtension(_currentImagePath) ?? "未命名图像";
    }
    
    /// <summary>
    /// 获取当前图像的备注信息
    /// </summary>
    /// <returns>备注信息</returns>
    public string? GetCurrentImageComment()
    {
        return _imageComment;
    }

    /// <summary>
    /// 显示验证错误对话框
    /// </summary>
    /// <param name="validationResult">验证结果</param>
    private void ShowValidationErrorDialog(ImageValidationResult validationResult)
    {
        string title = "图像文件验证失败";
        string message = validationResult.ErrorMessage ?? "未知错误";
        
        // 根据错误类型提供更详细的信息和建议
        string detailedMessage = validationResult.ErrorType switch
        {
            ImageValidationError.FileNotFound => 
                $"{message}\n\n请确认文件路径是否正确，文件是否存在。",
            
            ImageValidationError.UnsupportedFormat => 
                $"{message}\n\n当前支持的格式：PNG、JPG、JPEG、BMP\n请选择支持的图像格式。",
            
            ImageValidationError.FileSizeExceeded => 
                $"{message}\n\n建议：\n• 使用图像编辑软件压缩图像\n• 降低图像质量或分辨率\n• 选择其他更小的图像文件",
            
            // 移除ImageTooSmall验证错误处理
            
            ImageValidationError.ImageTooLarge => 
                $"{message}\n\n建议：\n• 使用图像编辑软件缩小图像\n• 裁剪图像到合适的区域\n• 降低图像分辨率",
            
            ImageValidationError.CorruptedFile => 
                $"{message}\n\n建议：\n• 重新下载或获取图像文件\n• 使用其他图像编辑软件打开并重新保存\n• 检查文件是否完整",
            
            _ => message
        };
        
        _notificationService.ShowError(detailedMessage, title);
        
        _logger.LogWarning("图像验证失败: {ErrorType} - {ErrorMessage}", 
            validationResult.ErrorType, validationResult.ErrorMessage);
    }

    #endregion

    #region IDisposable

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // 清理资源
        _currentImage?.Dispose();
    }

    #endregion
}