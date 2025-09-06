using System.Drawing;
using HonyWing.Core.Models;

namespace HonyWing.Core.Interfaces;

/// <summary>
/// 图像服务接口
/// </summary>
public interface IImageService
{
    /// <summary>
    /// 加载图像文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>图像对象</returns>
    Task<Bitmap?> LoadImageAsync(string filePath);

    /// <summary>
    /// 保存图像到文件
    /// </summary>
    /// <param name="image">图像对象</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="format">图像格式</param>
    /// <returns>是否成功</returns>
    Task<bool> SaveImageAsync(Bitmap image, string filePath, ImageFormat? format = null);

    /// <summary>
    /// 创建图像缩略图
    /// </summary>
    /// <param name="image">原始图像</param>
    /// <param name="maxWidth">最大宽度</param>
    /// <param name="maxHeight">最大高度</param>
    /// <param name="maintainAspectRatio">是否保持宽高比</param>
    /// <returns>缩略图</returns>
    Bitmap CreateThumbnail(Bitmap image, int maxWidth, int maxHeight, bool maintainAspectRatio = true);

    /// <summary>
    /// 裁剪图像
    /// </summary>
    /// <param name="image">原始图像</param>
    /// <param name="cropArea">裁剪区域</param>
    /// <returns>裁剪后的图像</returns>
    Bitmap CropImage(Bitmap image, Rectangle cropArea);

    /// <summary>
    /// 调整图像大小
    /// </summary>
    /// <param name="image">原始图像</param>
    /// <param name="newWidth">新宽度</param>
    /// <param name="newHeight">新高度</param>
    /// <param name="maintainAspectRatio">是否保持宽高比</param>
    /// <returns>调整后的图像</returns>
    Bitmap ResizeImage(Bitmap image, int newWidth, int newHeight, bool maintainAspectRatio = true);

    /// <summary>
    /// 转换图像格式
    /// </summary>
    /// <param name="image">原始图像</param>
    /// <param name="targetFormat">目标格式</param>
    /// <returns>转换后的图像</returns>
    Bitmap ConvertImageFormat(Bitmap image, System.Drawing.Imaging.PixelFormat targetFormat);

    /// <summary>
    /// 获取图像信息
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>图像信息</returns>
    Task<ImageInfo?> GetImageInfoAsync(string filePath);

    /// <summary>
    /// 验证图像文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否为有效的图像文件</returns>
    Task<bool> ValidateImageFileAsync(string filePath);

    /// <summary>
    /// 验证图像文件并返回详细结果
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>验证结果</returns>
    Task<ImageValidationResult> ValidateImageFileWithDetailsAsync(string filePath);

    /// <summary>
    /// 获取支持的图像格式
    /// </summary>
    /// <returns>支持的格式列表</returns>
    List<string> GetSupportedFormats();

    /// <summary>
    /// 从剪贴板获取图像
    /// </summary>
    /// <returns>剪贴板中的图像</returns>
    Bitmap? GetImageFromClipboard();

    /// <summary>
    /// 将图像复制到剪贴板
    /// </summary>
    /// <param name="image">要复制的图像</param>
    /// <returns>是否成功</returns>
    bool CopyImageToClipboard(Bitmap image);

    /// <summary>
    /// 批量处理图像
    /// </summary>
    /// <param name="imagePaths">图像路径列表</param>
    /// <param name="processor">处理函数</param>
    /// <param name="progress">进度报告</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>处理结果</returns>
    Task<List<ImageProcessResult>> BatchProcessImagesAsync(
        IEnumerable<string> imagePaths,
        Func<Bitmap, Task<Bitmap?>> processor,
        IProgress<BatchProcessProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 计算图像哈希值
    /// </summary>
    /// <param name="image">图像对象</param>
    /// <param name="algorithm">哈希算法</param>
    /// <returns>哈希值</returns>
    string ComputeImageHash(Bitmap image, ImageHashAlgorithm algorithm = ImageHashAlgorithm.MD5);

    /// <summary>
    /// 比较两个图像的相似度
    /// </summary>
    /// <param name="image1">图像1</param>
    /// <param name="image2">图像2</param>
    /// <param name="algorithm">比较算法</param>
    /// <returns>相似度（0-1）</returns>
    double CompareImages(Bitmap image1, Bitmap image2, ImageCompareAlgorithm algorithm = ImageCompareAlgorithm.Histogram);
}

/// <summary>
/// 图像信息
/// </summary>
public class ImageInfo
{
    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 图像宽度
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// 图像高度
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// 像素格式
    /// </summary>
    public string PixelFormat { get; set; } = string.Empty;

    /// <summary>
    /// 图像格式
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// 水平分辨率（DPI）
    /// </summary>
    public float HorizontalResolution { get; set; }

    /// <summary>
    /// 垂直分辨率（DPI）
    /// </summary>
    public float VerticalResolution { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime LastWriteTime { get; set; }

    /// <summary>
    /// 哈希值
    /// </summary>
    public string? Hash { get; set; }
}

/// <summary>
/// 图像验证结果
/// </summary>
public class ImageValidationResult
{
    /// <summary>
    /// 是否验证通过
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 验证失败的原因
    /// </summary>
    public ImageValidationError ErrorType { get; set; }

    /// <summary>
    /// 文件信息（如果文件存在）
    /// </summary>
    public ImageInfo? ImageInfo { get; set; }
}

/// <summary>
/// 图像验证错误类型
/// </summary>
public enum ImageValidationError
{
    /// <summary>
    /// 无错误
    /// </summary>
    None,

    /// <summary>
    /// 文件不存在
    /// </summary>
    FileNotFound,

    /// <summary>
    /// 不支持的格式
    /// </summary>
    UnsupportedFormat,

    /// <summary>
    /// 文件大小超过限制
    /// </summary>
    FileSizeExceeded,

    /// <summary>
    /// 图像尺寸过小
    /// </summary>
    ImageTooSmall,

    /// <summary>
    /// 图像尺寸过大
    /// </summary>
    ImageTooLarge,

    /// <summary>
    /// 文件损坏或无法读取
    /// </summary>
    CorruptedFile
}

/// <summary>
/// 图像处理结果
/// </summary>
public class ImageProcessResult
{
    /// <summary>
    /// 原始文件路径
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// 输出文件路径
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 处理耗时（毫秒）
    /// </summary>
    public long Duration { get; set; }
}

/// <summary>
/// 批量处理进度
/// </summary>
public class BatchProcessProgress
{
    /// <summary>
    /// 总数量
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 已处理数量
    /// </summary>
    public int Processed { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int Succeeded { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// 当前处理的文件
    /// </summary>
    public string? CurrentFile { get; set; }

    /// <summary>
    /// 进度百分比（0-100）
    /// </summary>
    public double ProgressPercentage => Total > 0 ? (double)Processed / Total * 100 : 0;
}

/// <summary>
/// 图像格式枚举
/// </summary>
public enum ImageFormat
{
    /// <summary>
    /// PNG格式
    /// </summary>
    Png,

    /// <summary>
    /// JPEG格式
    /// </summary>
    Jpeg,

    /// <summary>
    /// BMP格式
    /// </summary>
    Bmp,

    /// <summary>
    /// GIF格式
    /// </summary>
    Gif,

    /// <summary>
    /// TIFF格式
    /// </summary>
    Tiff
}

/// <summary>
/// 图像哈希算法
/// </summary>
public enum ImageHashAlgorithm
{
    /// <summary>
    /// MD5算法
    /// </summary>
    MD5,

    /// <summary>
    /// SHA1算法
    /// </summary>
    SHA1,

    /// <summary>
    /// SHA256算法
    /// </summary>
    SHA256,

    /// <summary>
    /// 感知哈希
    /// </summary>
    Perceptual
}

/// <summary>
/// 图像比较算法
/// </summary>
public enum ImageCompareAlgorithm
{
    /// <summary>
    /// 直方图比较
    /// </summary>
    Histogram,

    /// <summary>
    /// 结构相似性
    /// </summary>
    SSIM,

    /// <summary>
    /// 均方误差
    /// </summary>
    MSE,

    /// <summary>
    /// 感知哈希
    /// </summary>
    PerceptualHash
}