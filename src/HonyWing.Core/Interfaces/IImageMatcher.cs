using System.Drawing;

namespace HonyWing.Core.Interfaces;

/// <summary>
/// 图像匹配服务接口
/// </summary>
public interface IImageMatcher
{
    /// <summary>
    /// 在屏幕中查找目标图像
    /// </summary>
    /// <param name="templateImage">模板图像</param>
    /// <param name="threshold">匹配阈值 (0.0-1.0)</param>
    /// <returns>匹配结果，包含位置和相似度</returns>
    Task<MatchResult?> FindImageAsync(Bitmap templateImage, double threshold = 0.8);

    /// <summary>
    /// 在指定区域中查找目标图像
    /// </summary>
    /// <param name="templateImage">模板图像</param>
    /// <param name="searchArea">搜索区域</param>
    /// <param name="threshold">匹配阈值</param>
    /// <returns>匹配结果</returns>
    Task<MatchResult?> FindImageInAreaAsync(Bitmap templateImage, Rectangle searchArea, double threshold = 0.8);

    /// <summary>
    /// 查找所有匹配的图像位置
    /// </summary>
    /// <param name="templateImage">模板图像</param>
    /// <param name="threshold">匹配阈值</param>
    /// <returns>所有匹配结果</returns>
    Task<IEnumerable<MatchResult>> FindAllMatchesAsync(Bitmap templateImage, double threshold = 0.8);
}

/// <summary>
/// 图像匹配结果
/// </summary>
public class MatchResult
{
    /// <summary>
    /// 匹配位置
    /// </summary>
    public Point Location { get; set; }

    /// <summary>
    /// 匹配区域
    /// </summary>
    public Rectangle Rectangle { get; set; }

    /// <summary>
    /// 相似度 (0.0-1.0)
    /// </summary>
    public double Similarity { get; set; }

    /// <summary>
    /// 匹配时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}