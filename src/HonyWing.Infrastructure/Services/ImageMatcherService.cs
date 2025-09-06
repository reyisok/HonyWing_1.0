using HonyWing.Core.Interfaces;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace HonyWing.Infrastructure.Services;

/// <summary>
/// 图像匹配服务实现
/// </summary>
[SupportedOSPlatform("windows6.1")]
public class ImageMatcherService : IImageMatcher
{
    private readonly ILogger<ImageMatcherService> _logger;
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly IDpiAdaptationService _dpiAdaptationService;

    public ImageMatcherService(
        ILogger<ImageMatcherService> logger,
        IScreenCaptureService screenCaptureService,
        IDpiAdaptationService dpiAdaptationService)
    {
        _logger = logger;
        _screenCaptureService = screenCaptureService;
        _dpiAdaptationService = dpiAdaptationService;
    }

    public async Task<MatchResult?> FindImageAsync(Bitmap templateImage, double threshold = 0.8)
    {
        try
        {
            _logger.LogInformation("开始全屏图像匹配，阈值: {Threshold}", threshold);

            // 获取屏幕截图
            using var screenshot = await _screenCaptureService.CaptureFullScreenAsync();
            if (screenshot == null)
            {
                _logger.LogWarning("无法获取屏幕截图");
                return null;
            }

            _logger.LogDebug("屏幕截图尺寸: {Width}x{Height}, 模板图像尺寸: {TemplateWidth}x{TemplateHeight}", 
                screenshot.Width, screenshot.Height, templateImage.Width, templateImage.Height);

            // 执行图像匹配
            var result = await PerformTemplateMatchingAsync(screenshot, templateImage, threshold);

            if (result != null)
            {
                _logger.LogInformation("找到最佳匹配位置 ({X}, {Y})，置信度 {Confidence:F3}", result.Location.X, result.Location.Y, result.Similarity);
            }
            else
            {
                _logger.LogInformation("未找到匹配项，阈值 {Threshold}", threshold);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "图像匹配失败");
            throw;
        }
    }

    public async Task<MatchResult?> FindImageInAreaAsync(Bitmap templateImage, Rectangle searchArea, double threshold = 0.8)
    {
        try
        {
            // 应用DPI适配到搜索区域
            var dpiAdjustedSearchArea = _dpiAdaptationService.DipToPixel(searchArea);
            
            _logger.LogDebug("开始区域图像匹配，原始区域: ({X}, {Y}, {Width}, {Height}), DPI调整后区域: ({DpiX}, {DpiY}, {DpiWidth}, {DpiHeight}), 阈值: {Threshold}",
                searchArea.X, searchArea.Y, searchArea.Width, searchArea.Height,
                dpiAdjustedSearchArea.X, dpiAdjustedSearchArea.Y, dpiAdjustedSearchArea.Width, dpiAdjustedSearchArea.Height, threshold);

            // 获取指定区域的截图
            using var areaScreenshot = await _screenCaptureService.CaptureAreaAsync(dpiAdjustedSearchArea);
            if (areaScreenshot == null)
            {
                _logger.LogWarning("无法获取区域截图");
                return null;
            }

            // 执行图像匹配
            var result = await PerformTemplateMatchingAsync(areaScreenshot, templateImage, threshold);

            // 调整匹配结果的坐标（相对于搜索区域转换为屏幕坐标）
            if (result != null)
            {
                result.Location = new System.Drawing.Point(
                    result.Location.X + dpiAdjustedSearchArea.X,
                    result.Location.Y + dpiAdjustedSearchArea.Y);

                result.Rectangle = new Rectangle(
                    result.Rectangle.X + dpiAdjustedSearchArea.X,
                    result.Rectangle.Y + dpiAdjustedSearchArea.Y,
                    result.Rectangle.Width,
                    result.Rectangle.Height);

                _logger.LogInformation("区域图像匹配成功，位置: ({X}, {Y}), 相似度: {Similarity:P2}",
                    result.Location.X, result.Location.Y, result.Similarity);
            }
            else
            {
                _logger.LogDebug("在指定区域未找到匹配的图像");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "区域图像匹配过程中发生异常");
            throw;
        }
    }

    public async Task<IEnumerable<MatchResult>> FindAllMatchesAsync(Bitmap templateImage, double threshold = 0.8)
    {
        try
        {
            _logger.LogDebug("开始查找所有匹配项，阈值: {Threshold}", threshold);

            var results = new List<MatchResult>();

            // 获取屏幕截图
            using var screenshot = await _screenCaptureService.CaptureFullScreenAsync();
            if (screenshot == null)
            {
                _logger.LogWarning("无法获取屏幕截图");
                return results;
            }

            // 转换为OpenCV Mat
            using var sourceMat = BitmapToMat(screenshot);
            using var templateMat = BitmapToMat(templateImage);

            if (sourceMat.Empty() || templateMat.Empty())
            {
                _logger.LogWarning("图像转换失败");
                return results;
            }

            // 执行模板匹配
            using var resultMat = new Mat();
            Cv2.MatchTemplate(sourceMat, templateMat, resultMat, TemplateMatchModes.CCoeffNormed);

            // 查找所有匹配点
            var locations = new List<OpenCvSharp.Point>();
            var mask = Mat.Ones(resultMat.Size(), MatType.CV_8UC1);

            while (true)
            {
                Cv2.MinMaxLoc(resultMat, out _, out var maxVal, out _, out var maxLoc, mask);

                if (maxVal < threshold)
                    break;

                locations.Add(maxLoc);

                // 在已找到的位置周围创建掩码，避免重复检测
                var maskRect = new OpenCvSharp.Rect(
                    Math.Max(0, maxLoc.X - templateMat.Width / 2),
                    Math.Max(0, maxLoc.Y - templateMat.Height / 2),
                    Math.Min(templateMat.Width, mask.Size().Width - Math.Max(0, maxLoc.X - templateMat.Width / 2)),
                    Math.Min(templateMat.Height, mask.Size().Height - Math.Max(0, maxLoc.Y - templateMat.Height / 2)));

                Cv2.Rectangle(mask, maskRect, Scalar.All(0), -1);
            }

            // 转换为MatchResult，应用DPI适配
            foreach (var location in locations)
            {
                var similarity = resultMat.At<float>(location.Y, location.X);
                
                // 应用DPI适配，确保坐标精准
                var dipLocation = new System.Drawing.Point(location.X, location.Y);
                var pixelLocation = _dpiAdaptationService.DipToPixel(dipLocation);
                
                var dipRect = new Rectangle(location.X, location.Y, templateImage.Width, templateImage.Height);
                var pixelRect = _dpiAdaptationService.DipToPixel(dipRect);
                
                results.Add(new MatchResult
                {
                    Location = pixelLocation,
                    Rectangle = pixelRect,
                    Similarity = similarity,
                    Timestamp = DateTime.Now
                });
            }

            _logger.LogInformation("图像匹配完成，找到 {Count} 个匹配项", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查找最佳匹配失败");
            throw;
        }
    }

    private async Task<MatchResult?> PerformTemplateMatchingAsync(Bitmap sourceImage, Bitmap templateImage, double threshold)
    {
        return await Task.Run(() =>
        {
            try
            {
                // 转换为OpenCV Mat
                using var sourceMat = BitmapToMat(sourceImage);
                using var templateMat = BitmapToMat(templateImage);

                if (sourceMat.Empty() || templateMat.Empty())
                {
                    _logger.LogWarning("图像转换失败");
                    return null;
                }

                // 检查模板图像是否大于源图像
                if (templateMat.Width > sourceMat.Width || templateMat.Height > sourceMat.Height)
                {
                    _logger.LogWarning("模板图像大于源图像，无法进行匹配");
                    return null;
                }

                // 执行模板匹配
                using var resultMat = new Mat();
                Cv2.MatchTemplate(sourceMat, templateMat, resultMat, TemplateMatchModes.CCoeffNormed);

                // 查找最佳匹配位置
                Cv2.MinMaxLoc(resultMat, out _, out var maxVal, out _, out var maxLoc);

                _logger.LogDebug("模板匹配完成，最大相似度: {MaxVal:F4}, 位置: ({X}, {Y})", maxVal, maxLoc.X, maxLoc.Y);

                // 检查是否达到阈值
                if (maxVal >= threshold)
                {
                    // 模板匹配返回的坐标已经是屏幕像素坐标，无需DPI转换
                    var matchLocation = new System.Drawing.Point(maxLoc.X, maxLoc.Y);
                    var matchRect = new Rectangle(maxLoc.X, maxLoc.Y, templateImage.Width, templateImage.Height);
                    
                    _logger.LogDebug("匹配成功: 位置({X}, {Y}), 尺寸({Width}x{Height}), 相似度: {Similarity:F4}", 
                        matchLocation.X, matchLocation.Y, matchRect.Width, matchRect.Height, maxVal);
                    
                    return new MatchResult
                    {
                        Location = matchLocation,
                        Rectangle = matchRect,
                        Similarity = maxVal,
                        Timestamp = DateTime.Now
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "模板匹配执行失败");
                throw;
            }
        });
    }

    private static Mat BitmapToMat(Bitmap bitmap)
    {
        try
        {
            // 锁定位图数据
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                // 创建Mat对象
                var mat = new Mat(bitmap.Height, bitmap.Width, MatType.CV_8UC3, bitmapData.Scan0, bitmapData.Stride);

                // 复制数据以避免内存访问问题
                var result = mat.Clone();

                // BGR转RGB（OpenCV使用BGR，.NET使用RGB）
                Cv2.CvtColor(result, result, ColorConversionCodes.BGR2RGB);

                return result;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("位图转换为Mat失败", ex);
        }
    }
}
