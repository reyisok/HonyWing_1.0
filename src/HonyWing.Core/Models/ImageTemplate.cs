using System.Drawing;

namespace HonyWing.Core.Models;

/// <summary>
/// 图像模板模型
/// </summary>
public class ImageTemplate
{
    /// <summary>
    /// 模板ID
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 图像文件路径
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 匹配阈值
    /// </summary>
    public double Threshold { get; set; } = 0.8;

    /// <summary>
    /// 搜索区域（可选）
    /// </summary>
    public Rectangle? SearchArea { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// 使用次数
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// 获取图像对象
    /// </summary>
    /// <returns>图像对象，如果文件不存在则返回null</returns>
    public Bitmap? GetImage()
    {
        if (string.IsNullOrEmpty(ImagePath) || !File.Exists(ImagePath))
            return null;

        try
        {
#pragma warning disable CA1416 // 验证平台兼容性
            return new Bitmap(ImagePath);
#pragma warning restore CA1416 // 验证平台兼容性
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 更新使用统计
    /// </summary>
    public void UpdateUsageStats()
    {
        UsageCount++;
        LastUsedAt = DateTime.Now;
        ModifiedAt = DateTime.Now;
    }
}