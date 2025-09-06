using HonyWing.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageFormat = HonyWing.Core.Interfaces.ImageFormat;
using DrawingPoint = System.Drawing.Point;
using WinFormsClipboard = System.Windows.Forms.Clipboard;

namespace HonyWing.Infrastructure.Services;

/// <summary>
/// 图像服务实现
/// </summary>
[SupportedOSPlatform("windows6.1")]
public class ImageService : IImageService
{
    private readonly ILogger<ImageService> _logger;
    private static readonly string[] SupportedExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    // 移除最小图像尺寸限制，允许任意尺寸的图像
    private const int MaxImageWidth = 1920;
    private const int MaxImageHeight = 1080;

    public ImageService(ILogger<ImageService> logger)
    {
        _logger = logger;
        _logger.LogInformation("图像服务初始化完成");
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<Bitmap?> LoadImageAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogDebug("加载图像文件: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("图像文件不存在: {FilePath}", filePath);
                    return null;
                }

                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var originalImage = new Bitmap(fileStream);
                
                // 创建副本以避免文件锁定
                var bitmap = new Bitmap(originalImage);
                originalImage.Dispose();

                _logger.LogInformation("图像加载成功: {FilePath}, 尺寸: {Width}x{Height}", 
                    filePath, bitmap.Width, bitmap.Height);
                
                return bitmap;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "图像加载失败: {FilePath}", filePath);
                return null;
            }
        });
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<bool> SaveImageAsync(Bitmap image, string filePath, ImageFormat? format = null)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogDebug("保存图像到文件: {FilePath}", filePath);

                // 确保目录存在
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 确定图像格式
                var imageFormat = GetSystemImageFormat(format, filePath);
                
                image.Save(filePath, imageFormat);

                _logger.LogInformation("图像保存成功: {FilePath}, 格式: {Format}", filePath, imageFormat);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "图像保存失败: {FilePath}", filePath);
                return false;
            }
        });
    }

    [SupportedOSPlatform("windows6.1")]
    public Bitmap CreateThumbnail(Bitmap image, int maxWidth, int maxHeight, bool maintainAspectRatio = true)
    {
        try
        {
            _logger.LogDebug("创建缩略图，目标尺寸: {MaxWidth}x{MaxHeight}", maxWidth, maxHeight);

            int newWidth, newHeight;

            if (maintainAspectRatio)
            {
                var aspectRatio = (double)image.Width / image.Height;
                
                if (aspectRatio > (double)maxWidth / maxHeight)
                {
                    newWidth = maxWidth;
                    newHeight = (int)(maxWidth / aspectRatio);
                }
                else
                {
                    newHeight = maxHeight;
                    newWidth = (int)(maxHeight * aspectRatio);
                }
            }
            else
            {
                newWidth = maxWidth;
                newHeight = maxHeight;
            }

            var thumbnail = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(thumbnail))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            _logger.LogDebug("缩略图创建完成，实际尺寸: {Width}x{Height}", newWidth, newHeight);
            return thumbnail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建缩略图失败");
            throw;
        }
    }

    [SupportedOSPlatform("windows6.1")]
    public Bitmap CropImage(Bitmap image, Rectangle cropArea)
    {
        try
        {
            _logger.LogDebug("裁剪图像，区域: ({X}, {Y}, {Width}, {Height})", 
                cropArea.X, cropArea.Y, cropArea.Width, cropArea.Height);

            // 验证裁剪区域
            var validCropArea = Rectangle.Intersect(cropArea, new Rectangle(0, 0, image.Width, image.Height));
            if (validCropArea.IsEmpty)
            {
                throw new ArgumentException("裁剪区域无效或超出图像范围");
            }

            var croppedImage = new Bitmap(validCropArea.Width, validCropArea.Height);
            using (var graphics = Graphics.FromImage(croppedImage))
            {
                graphics.DrawImage(image, new Rectangle(0, 0, validCropArea.Width, validCropArea.Height),
                    validCropArea, GraphicsUnit.Pixel);
            }

            _logger.LogDebug("图像裁剪完成");
            return croppedImage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "裁剪图像失败");
            throw;
        }
    }

    [SupportedOSPlatform("windows6.1")]
    public Bitmap ResizeImage(Bitmap image, int newWidth, int newHeight, bool maintainAspectRatio = true)
    {
        try
        {
            _logger.LogDebug("调整图像大小到: {NewWidth}x{NewHeight}", newWidth, newHeight);

            int targetWidth, targetHeight;

            if (maintainAspectRatio)
            {
                var aspectRatio = (double)image.Width / image.Height;
                var targetAspectRatio = (double)newWidth / newHeight;

                if (aspectRatio > targetAspectRatio)
                {
                    targetWidth = newWidth;
                    targetHeight = (int)(newWidth / aspectRatio);
                }
                else
                {
                    targetHeight = newHeight;
                    targetWidth = (int)(newHeight * aspectRatio);
                }
            }
            else
            {
                targetWidth = newWidth;
                targetHeight = newHeight;
            }

            var resizedImage = new Bitmap(targetWidth, targetHeight);
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, targetWidth, targetHeight);
            }

            _logger.LogInformation("图像缩放成功: {OriginalWidth}x{OriginalHeight} -> {NewWidth}x{NewHeight}",
                image.Width, image.Height, targetWidth, targetHeight);
            return resizedImage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "图像缩放失败");
            throw;
        }
    }

    [SupportedOSPlatform("windows6.1")]
    public Bitmap ConvertImageFormat(Bitmap image, PixelFormat targetFormat)
    {
        try
        {
            _logger.LogDebug("转换图像格式到: {TargetFormat}", targetFormat);

            var convertedImage = new Bitmap(image.Width, image.Height, targetFormat);
            using (var graphics = Graphics.FromImage(convertedImage))
            {
                graphics.DrawImage(image, 0, 0);
            }

            _logger.LogDebug("图像格式转换完成");
            return convertedImage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "转换图像格式失败");
            throw;
        }
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<ImageInfo?> GetImageInfoAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogDebug("获取图像信息: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    return null;
                }

                var fileInfo = new FileInfo(filePath);
                using var image = Image.FromFile(filePath);

                var imageInfo = new ImageInfo
                {
                    FilePath = filePath,
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    Width = image.Width,
                    Height = image.Height,
                    PixelFormat = image.PixelFormat.ToString(),
                    Format = image.RawFormat.ToString(),
                    HorizontalResolution = image.HorizontalResolution,
                    VerticalResolution = image.VerticalResolution,
                    CreationTime = fileInfo.CreationTime,
                    LastWriteTime = fileInfo.LastWriteTime
                };

                _logger.LogDebug("图像信息获取完成");
                return imageInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取图像信息失败: {FilePath}", filePath);
                return null;
            }
        });
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<bool> ValidateImageFileAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("文件不存在: {FilePath}", filePath);
                    return false;
                }

                // 检查文件扩展名
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                if (!SupportedExtensions.Contains(extension))
                {
                    _logger.LogWarning("不支持的文件格式: {Extension}", extension);
                    return false;
                }

                // 检查文件大小
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > MaxFileSizeBytes)
                {
                    _logger.LogWarning("文件大小超过限制: {FileSize}MB, 最大允许: {MaxSize}MB", 
                        fileInfo.Length / (1024.0 * 1024.0), MaxFileSizeBytes / (1024.0 * 1024.0));
                    return false;
                }

                // 尝试加载图像以验证格式和尺寸
                using var image = Image.FromFile(filePath);
                
                // 移除最小图像尺寸检查，允许任意尺寸的图像
                
                if (image.Width > MaxImageWidth || image.Height > MaxImageHeight)
                {
                    _logger.LogWarning("图像尺寸过大: {Width}x{Height}, 最大允许: {MaxWidth}x{MaxHeight}", 
                        image.Width, image.Height, MaxImageWidth, MaxImageHeight);
                    return false;
                }

                _logger.LogDebug("图像文件验证通过: {FilePath}, 尺寸: {Width}x{Height}, 大小: {FileSize}KB", 
                    filePath, image.Width, image.Height, fileInfo.Length / 1024.0);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证图像文件时发生错误: {FilePath}", filePath);
                return false;
            }
        });
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<ImageValidationResult> ValidateImageFileWithDetailsAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var result = new ImageValidationResult();
            
            try
            {
                // 检查文件是否存在
                if (!File.Exists(filePath))
                {
                    result.IsValid = false;
                    result.ErrorType = ImageValidationError.FileNotFound;
                    result.ErrorMessage = "文件不存在";
                    _logger.LogWarning("文件不存在: {FilePath}", filePath);
                    return result;
                }

                // 检查文件扩展名
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                if (!SupportedExtensions.Contains(extension))
                {
                    result.IsValid = false;
                    result.ErrorType = ImageValidationError.UnsupportedFormat;
                    result.ErrorMessage = $"仅支持PNG/JPG/BMP格式，请重新选择";
                    _logger.LogWarning("不支持的文件格式: {Extension}", extension);
                    return result;
                }

                // 检查文件大小
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > MaxFileSizeBytes)
                {
                    result.IsValid = false;
                    result.ErrorType = ImageValidationError.FileSizeExceeded;
                    result.ErrorMessage = $"文件大小超过限制，最大允许10MB";
                    _logger.LogWarning("文件大小超过限制: {FileSize}MB, 最大允许: {MaxSize}MB", 
                        fileInfo.Length / (1024.0 * 1024.0), MaxFileSizeBytes / (1024.0 * 1024.0));
                    return result;
                }

                // 尝试加载图像以验证格式和尺寸
                using var image = Image.FromFile(filePath);
                
                // 移除最小图像尺寸检查，允许任意尺寸的图像
                
                if (image.Width > MaxImageWidth || image.Height > MaxImageHeight)
                {
                    result.IsValid = false;
                    result.ErrorType = ImageValidationError.ImageTooLarge;
                    result.ErrorMessage = $"图像尺寸过大，最大允许{MaxImageWidth}x{MaxImageHeight}像素";
                    _logger.LogWarning("图像尺寸过大: {Width}x{Height}, 最大允许: {MaxWidth}x{MaxHeight}", 
                        image.Width, image.Height, MaxImageWidth, MaxImageHeight);
                    return result;
                }

                // 创建图像信息
                result.ImageInfo = new ImageInfo
                {
                    FilePath = filePath,
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    Width = image.Width,
                    Height = image.Height,
                    PixelFormat = image.PixelFormat.ToString(),
                    Format = image.RawFormat.ToString(),
                    HorizontalResolution = image.HorizontalResolution,
                    VerticalResolution = image.VerticalResolution,
                    CreationTime = fileInfo.CreationTime,
                    LastWriteTime = fileInfo.LastWriteTime
                };

                result.IsValid = true;
                result.ErrorType = ImageValidationError.None;
                
                _logger.LogDebug("图像文件验证通过: {FilePath}, 尺寸: {Width}x{Height}, 大小: {FileSize}KB", 
                    filePath, image.Width, image.Height, fileInfo.Length / 1024.0);
                
                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorType = ImageValidationError.CorruptedFile;
                result.ErrorMessage = "文件损坏或无法读取";
                _logger.LogError(ex, "验证图像文件时发生错误: {FilePath}", filePath);
                return result;
            }
        });
    }

    [SupportedOSPlatform("windows6.1")]
    public List<string> GetSupportedFormats()
    {
        return SupportedExtensions.ToList();
    }

    [SupportedOSPlatform("windows6.1")]
    public Bitmap? GetImageFromClipboard()
    {
        try
        {
            _logger.LogDebug("从剪贴板获取图像");

            if (WinFormsClipboard.ContainsImage())
            {
                var clipboardImage = WinFormsClipboard.GetImage();
                if (clipboardImage != null)
                {
                    var bitmap = new Bitmap(clipboardImage);
                    _logger.LogInformation("成功从剪贴板获取图像，尺寸: {Width}x{Height}", 
                        bitmap.Width, bitmap.Height);
                    return bitmap;
                }
            }

            _logger.LogDebug("剪贴板中没有图像");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从剪贴板获取图像失败");
            return null;
        }
    }

    [SupportedOSPlatform("windows6.1")]
    public bool CopyImageToClipboard(Bitmap image)
    {
        try
        {
            _logger.LogDebug("将图像复制到剪贴板");

            WinFormsClipboard.SetImage(image);

            _logger.LogInformation("成功将图像复制到剪贴板");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "复制图像到剪贴板失败");
            return false;
        }
    }

    [SupportedOSPlatform("windows6.1")]
    public async Task<List<ImageProcessResult>> BatchProcessImagesAsync(
        IEnumerable<string> imagePaths,
        Func<Bitmap, Task<Bitmap?>> processor,
        IProgress<BatchProcessProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ImageProcessResult>();
        var pathList = imagePaths.ToList();
        var total = pathList.Count;
        var processed = 0;
        var succeeded = 0;
        var failed = 0;

        _logger.LogInformation("开始批量处理图像，总数: {Total}", total);

        foreach (var imagePath in pathList)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var result = new ImageProcessResult
            {
                SourcePath = imagePath
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                progress?.Report(new BatchProcessProgress
                {
                    Total = total,
                    Processed = processed,
                    Succeeded = succeeded,
                    Failed = failed,
                    CurrentFile = Path.GetFileName(imagePath)
                });

                using var originalImage = await LoadImageAsync(imagePath);
                if (originalImage != null)
                {
                    var processedImage = await processor(originalImage);
                    if (processedImage != null)
                    {
                        result.Success = true;
                        succeeded++;
                        processedImage.Dispose();
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = "处理器返回null";
                        failed++;
                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "无法加载图像";
                    failed++;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                failed++;
                _logger.LogError(ex, "批量处理图像失败: {ImagePath}", imagePath);
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = stopwatch.ElapsedMilliseconds;
                results.Add(result);
                processed++;
            }
        }

        progress?.Report(new BatchProcessProgress
        {
            Total = total,
            Processed = processed,
            Succeeded = succeeded,
            Failed = failed
        });

        _logger.LogInformation("批量处理完成，成功: {Succeeded}, 失败: {Failed}", succeeded, failed);
        return results;
    }

    [SupportedOSPlatform("windows6.1")]
    public string ComputeImageHash(Bitmap image, ImageHashAlgorithm algorithm = ImageHashAlgorithm.MD5)
    {
        try
        {
            _logger.LogDebug("计算图像哈希，算法: {Algorithm}", algorithm);

            using var memoryStream = new MemoryStream();
            image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            var imageBytes = memoryStream.ToArray();

            return algorithm switch
            {
                ImageHashAlgorithm.MD5 => ComputeHash(imageBytes, MD5.Create()),
                ImageHashAlgorithm.SHA1 => ComputeHash(imageBytes, SHA1.Create()),
                ImageHashAlgorithm.SHA256 => ComputeHash(imageBytes, SHA256.Create()),
                ImageHashAlgorithm.Perceptual => ComputePerceptualHash(image),
                _ => ComputeHash(imageBytes, MD5.Create())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "计算图像哈希失败");
            throw;
        }
    }

    [SupportedOSPlatform("windows6.1")]
    public double CompareImages(Bitmap image1, Bitmap image2, ImageCompareAlgorithm algorithm = ImageCompareAlgorithm.Histogram)
    {
        try
        {
            _logger.LogDebug("比较图像相似度，算法: {Algorithm}", algorithm);

            return algorithm switch
            {
                ImageCompareAlgorithm.Histogram => CompareHistograms(image1, image2),
                ImageCompareAlgorithm.MSE => ComputeMSE(image1, image2),
                ImageCompareAlgorithm.SSIM => ComputeSSIM(image1, image2),
                ImageCompareAlgorithm.PerceptualHash => ComparePerceptualHashes(image1, image2),
                _ => CompareHistograms(image1, image2)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "比较图像相似度失败");
            throw;
        }
    }

    #region Private Methods

    private static System.Drawing.Imaging.ImageFormat GetSystemImageFormat(ImageFormat? format, string filePath)
    {
        if (format.HasValue)
        {
            return format.Value switch
            {
                ImageFormat.Png => System.Drawing.Imaging.ImageFormat.Png,
                ImageFormat.Jpeg => System.Drawing.Imaging.ImageFormat.Jpeg,
                ImageFormat.Bmp => System.Drawing.Imaging.ImageFormat.Bmp,
                ImageFormat.Gif => System.Drawing.Imaging.ImageFormat.Gif,
                ImageFormat.Tiff => System.Drawing.Imaging.ImageFormat.Tiff,
                _ => System.Drawing.Imaging.ImageFormat.Png
            };
        }

        // 根据文件扩展名确定格式
        string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
        if (fileExtension == ".jpg" || fileExtension == ".jpeg")
            return System.Drawing.Imaging.ImageFormat.Jpeg;
        if (fileExtension == ".bmp")
            return System.Drawing.Imaging.ImageFormat.Bmp;
        if (fileExtension == ".gif")
            return System.Drawing.Imaging.ImageFormat.Gif;
        if (fileExtension == ".tiff" || fileExtension == ".tif")
            return System.Drawing.Imaging.ImageFormat.Tiff;
        return System.Drawing.Imaging.ImageFormat.Png;
    }

    private static string ComputeHash(byte[] data, HashAlgorithm hashAlgorithm)
    {
        using (hashAlgorithm)
        {
            var hash = hashAlgorithm.ComputeHash(data);
            return Convert.ToHexString(hash);
        }
    }

    private static string ComputePerceptualHash(Bitmap image)
    {
        // 简化的感知哈希实现
        using var resized = new Bitmap(8, 8);
        using (var graphics = Graphics.FromImage(resized))
        {
            graphics.DrawImage(image, 0, 0, 8, 8);
        }

        var pixels = new byte[64];
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                var pixel = resized.GetPixel(x, y);
                pixels[y * 8 + x] = (byte)((pixel.R + pixel.G + pixel.B) / 3);
            }
        }

        var average = pixels.Select(p => (double)p).Average();
        var hash = 0UL;
        for (int i = 0; i < 64; i++)
        {
            if (pixels[i] > average)
            {
                hash |= 1UL << i;
            }
        }

        return hash.ToString("X16");
    }

    private static double CompareHistograms(Bitmap image1, Bitmap image2)
    {
        // 简化的直方图比较实现
        var hist1 = ComputeHistogram(image1);
        var hist2 = ComputeHistogram(image2);

        double correlation = 0;
        for (int i = 0; i < 256; i++)
        {
            correlation += Math.Min(hist1[i], hist2[i]);
        }

        return correlation;
    }

    private static double[] ComputeHistogram(Bitmap image)
    {
        var histogram = new double[256];
        var totalPixels = image.Width * image.Height;

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image.GetPixel(x, y);
                var gray = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                histogram[gray]++;
            }
        }

        for (int i = 0; i < 256; i++)
        {
            histogram[i] /= totalPixels;
        }

        return histogram;
    }

    private static double ComputeMSE(Bitmap image1, Bitmap image2)
    {
        if (image1.Width != image2.Width || image1.Height != image2.Height)
        {
            return double.MaxValue;
        }

        double mse = 0;
        var totalPixels = image1.Width * image1.Height;

        for (int y = 0; y < image1.Height; y++)
        {
            for (int x = 0; x < image1.Width; x++)
            {
                var pixel1 = image1.GetPixel(x, y);
                var pixel2 = image2.GetPixel(x, y);

                var diff = Math.Pow(pixel1.R - pixel2.R, 2) + 
                          Math.Pow(pixel1.G - pixel2.G, 2) + 
                          Math.Pow(pixel1.B - pixel2.B, 2);
                mse += diff;
            }
        }

        return 1.0 - (mse / (totalPixels * 3 * 255 * 255));
    }

    private static double ComputeSSIM(Bitmap image1, Bitmap image2)
    {
        // 简化的SSIM实现
        return CompareHistograms(image1, image2);
    }

    private static double ComparePerceptualHashes(Bitmap image1, Bitmap image2)
    {
        var hash1 = ComputePerceptualHash(image1);
        var hash2 = ComputePerceptualHash(image2);

        if (ulong.TryParse(hash1, System.Globalization.NumberStyles.HexNumber, null, out var h1) &&
            ulong.TryParse(hash2, System.Globalization.NumberStyles.HexNumber, null, out var h2))
        {
            var xor = h1 ^ h2;
            var hammingDistance = CountBits(xor);
            return 1.0 - (hammingDistance / 64.0);
        }

        return 0;
    }

    private static int CountBits(ulong value)
    {
        int count = 0;
        while (value != 0)
        {
            count++;
            value &= value - 1;
        }
        return count;
    }

    private static Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
    {
        var bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, PixelFormat.Format32bppPArgb);
        var bitmapData = bitmap.LockBits(new Rectangle(DrawingPoint.Empty, bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
        bitmapSource.CopyPixels(System.Windows.Int32Rect.Empty, bitmapData.Scan0, bitmapData.Height * bitmapData.Stride, bitmapData.Stride);
        bitmap.UnlockBits(bitmapData);
        return bitmap;
    }

    private static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
    {
        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        var bitmapSource = BitmapSource.Create(bitmap.Width, bitmap.Height, bitmap.HorizontalResolution, bitmap.VerticalResolution,
            System.Windows.Media.PixelFormats.Bgra32, null, bitmapData.Scan0, bitmapData.Stride * bitmap.Height, bitmapData.Stride);
        bitmap.UnlockBits(bitmapData);
        return bitmapSource;
    }

    /// <summary>
    /// 创建空白图像
    /// </summary>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="backgroundColor">背景颜色</param>
    /// <returns>空白图像</returns>
    [SupportedOSPlatform("windows6.1")]
    public Bitmap CreateBlankImage(int width, int height, Color backgroundColor = default)
    {
        try
        {
            _logger.LogDebug("创建空白图像，尺寸: {Width}x{Height}", width, height);

            if (backgroundColor == default)
            {
                backgroundColor = Color.White;
            }

            var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(backgroundColor);
            }

            _logger.LogDebug("空白图像创建完成");
            return bitmap;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建空白图像失败");
            throw;
        }
    }

    /// <summary>
    /// 获取图像主要颜色
    /// </summary>
    /// <param name="image">图像对象</param>
    /// <param name="colorCount">返回的颜色数量</param>
    /// <returns>主要颜色列表</returns>
    [SupportedOSPlatform("windows6.1")]
    public List<Color> GetDominantColors(Bitmap image, int colorCount = 5)
    {
        try
        {
            _logger.LogDebug("获取图像主要颜色，颜色数量: {ColorCount}", colorCount);

            var colorMap = new Dictionary<Color, int>();
            
            // 缩小图像以提高性能
            using var resized = CreateThumbnail(image, 100, 100, true);
            
            for (int y = 0; y < resized.Height; y++)
            {
                for (int x = 0; x < resized.Width; x++)
                {
                    var pixel = resized.GetPixel(x, y);
                    if (colorMap.ContainsKey(pixel))
                    {
                        colorMap[pixel]++;
                    }
                    else
                    {
                        colorMap[pixel] = 1;
                    }
                }
            }

            var dominantColors = colorMap
                .OrderByDescending(kvp => kvp.Value)
                .Take(colorCount)
                .Select(kvp => kvp.Key)
                .ToList();

            _logger.LogDebug("主要颜色获取完成，找到 {Count} 种颜色", dominantColors.Count);
            return dominantColors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取图像主要颜色失败");
            throw;
        }
    }

    /// <summary>
    /// 检测图像边缘
    /// </summary>
    /// <param name="image">输入图像</param>
    /// <returns>边缘检测结果</returns>
    [SupportedOSPlatform("windows6.1")]
    public Bitmap DetectEdges(Bitmap image)
    {
        try
        {
            _logger.LogDebug("检测图像边缘");

            // 先转换为灰度图像
            var grayImage = new Bitmap(image.Width, image.Height);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = image.GetPixel(x, y);
                    var gray = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                    var grayColor = Color.FromArgb(gray, gray, gray);
                    grayImage.SetPixel(x, y, grayColor);
                }
            }

            // 应用Sobel边缘检测
            var edgeImage = new Bitmap(image.Width, image.Height);
            for (int y = 1; y < image.Height - 1; y++)
            {
                for (int x = 1; x < image.Width - 1; x++)
                {
                    var gx = -grayImage.GetPixel(x - 1, y - 1).R + grayImage.GetPixel(x + 1, y - 1).R +
                            -2 * grayImage.GetPixel(x - 1, y).R + 2 * grayImage.GetPixel(x + 1, y).R +
                            -grayImage.GetPixel(x - 1, y + 1).R + grayImage.GetPixel(x + 1, y + 1).R;

                    var gy = -grayImage.GetPixel(x - 1, y - 1).R - 2 * grayImage.GetPixel(x, y - 1).R - grayImage.GetPixel(x + 1, y - 1).R +
                            grayImage.GetPixel(x - 1, y + 1).R + 2 * grayImage.GetPixel(x, y + 1).R + grayImage.GetPixel(x + 1, y + 1).R;

                    var magnitude = (int)Math.Sqrt(gx * gx + gy * gy);
                    magnitude = Math.Min(255, Math.Max(0, magnitude));
                    
                    var edgeColor = Color.FromArgb(magnitude, magnitude, magnitude);
                    edgeImage.SetPixel(x, y, edgeColor);
                }
            }

            grayImage.Dispose();
            _logger.LogDebug("图像边缘检测完成");
            return edgeImage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检测图像边缘失败");
            throw;
        }
    }

    /// <summary>
    /// 应用高斯模糊
    /// </summary>
    /// <param name="source">源图像</param>
    /// <param name="sigma">模糊强度</param>
    /// <returns>模糊后的图像</returns>
    [SupportedOSPlatform("windows6.1")]
    public Bitmap ApplyGaussianBlur(Bitmap source, double sigma)
    {
        try
        {
            _logger.LogDebug("应用高斯模糊，强度: {Sigma}", sigma);

            var kernelSize = (int)(6 * sigma + 1);
            if (kernelSize % 2 == 0) kernelSize++;
            
            var kernel = new double[kernelSize, kernelSize];
            var sum = 0.0;
            var center = kernelSize / 2;

            // 生成高斯核
            for (int i = 0; i < kernelSize; i++)
            {
                for (int j = 0; j < kernelSize; j++)
                {
                    var x = i - center;
                    var y = j - center;
                    kernel[i, j] = Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
                    sum += kernel[i, j];
                }
            }

            // 归一化核
            for (int i = 0; i < kernelSize; i++)
            {
                for (int j = 0; j < kernelSize; j++)
                {
                    kernel[i, j] /= sum;
                }
            }

            var result = new Bitmap(source.Width, source.Height);
            
            // 应用卷积
            for (int y = center; y < source.Height - center; y++)
            {
                for (int x = center; x < source.Width - center; x++)
                {
                    double r = 0, g = 0, b = 0;
                    
                    for (int ky = 0; ky < kernelSize; ky++)
                    {
                        for (int kx = 0; kx < kernelSize; kx++)
                        {
                            var pixel = source.GetPixel(x + kx - center, y + ky - center);
                            var weight = kernel[ky, kx];
                            
                            r += pixel.R * weight;
                            g += pixel.G * weight;
                            b += pixel.B * weight;
                        }
                    }
                    
                    var newColor = Color.FromArgb(
                        Math.Min(255, Math.Max(0, (int)r)),
                        Math.Min(255, Math.Max(0, (int)g)),
                        Math.Min(255, Math.Max(0, (int)b))
                    );
                    
                    result.SetPixel(x, y, newColor);
                }
            }

            _logger.LogDebug("高斯模糊应用完成");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "应用高斯模糊失败");
            throw;
        }
    }

    #endregion
}