using HonyWing.Core.Models;
using System.Collections.ObjectModel;

namespace HonyWing.Core.Interfaces;

/// <summary>
/// 图像模板服务接口
/// </summary>
public interface IImageTemplateService
{
    /// <summary>
    /// 模板集合
    /// </summary>
    ObservableCollection<ImageTemplate> Templates { get; }

    /// <summary>
    /// 模板添加事件
    /// </summary>
    event EventHandler<ImageTemplate>? TemplateAdded;

    /// <summary>
    /// 模板更新事件
    /// </summary>
    event EventHandler<ImageTemplate>? TemplateUpdated;

    /// <summary>
    /// 模板删除事件
    /// </summary>
    event EventHandler<string>? TemplateRemoved;

    /// <summary>
    /// 模板清空事件
    /// </summary>
    event EventHandler? TemplatesCleared;

    /// <summary>
    /// 创建图像模板
    /// </summary>
    /// <param name="name">模板名称</param>
    /// <param name="imagePath">图像文件路径</param>
    /// <param name="description">模板描述</param>
    /// <returns>创建的模板，失败返回null</returns>
    Task<ImageTemplate?> CreateTemplateAsync(string name, string imagePath, string? description = null);

    /// <summary>
    /// 从剪贴板创建图像模板
    /// </summary>
    /// <param name="name">模板名称</param>
    /// <param name="description">模板描述</param>
    /// <returns>创建的模板，失败返回null</returns>
    Task<ImageTemplate?> CreateTemplateFromClipboardAsync(string name, string? description = null);

    /// <summary>
    /// 更新图像模板
    /// </summary>
    /// <param name="template">要更新的模板</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateTemplateAsync(ImageTemplate template);

    /// <summary>
    /// 删除图像模板
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteTemplateAsync(string templateId);

    /// <summary>
    /// 获取指定ID的模板
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>模板对象，不存在返回null</returns>
    ImageTemplate? GetTemplate(string templateId);

    /// <summary>
    /// 根据名称获取模板
    /// </summary>
    /// <param name="name">模板名称</param>
    /// <returns>模板对象，不存在返回null</returns>
    ImageTemplate? GetTemplateByName(string name);

    /// <summary>
    /// 根据标签获取模板列表
    /// </summary>
    /// <param name="tag">标签</param>
    /// <returns>匹配的模板列表</returns>
    List<ImageTemplate> GetTemplatesByTag(string tag);

    /// <summary>
    /// 搜索模板
    /// </summary>
    /// <param name="keyword">关键词</param>
    /// <returns>匹配的模板列表</returns>
    List<ImageTemplate> SearchTemplates(string keyword);

    /// <summary>
    /// 导入模板
    /// </summary>
    /// <param name="filePath">模板文件路径</param>
    /// <returns>是否导入成功</returns>
    Task<bool> ImportTemplatesAsync(string filePath);

    /// <summary>
    /// 导出模板
    /// </summary>
    /// <param name="filePath">导出文件路径</param>
    /// <param name="templateIds">要导出的模板ID列表，null表示导出所有</param>
    /// <returns>是否导出成功</returns>
    Task<bool> ExportTemplatesAsync(string filePath, IEnumerable<string>? templateIds = null);

    /// <summary>
    /// 清空所有模板
    /// </summary>
    Task ClearTemplatesAsync();

    /// <summary>
    /// 更新模板使用统计
    /// </summary>
    /// <param name="templateId">模板ID</param>
    void UpdateUsageStatistics(string templateId);

    /// <summary>
    /// 获取最近使用的模板
    /// </summary>
    /// <param name="count">返回数量</param>
    /// <returns>最近使用的模板列表</returns>
    List<ImageTemplate> GetRecentlyUsedTemplates(int count = 10);

    /// <summary>
    /// 获取最常使用的模板
    /// </summary>
    /// <param name="count">返回数量</param>
    /// <returns>最常使用的模板列表</returns>
    List<ImageTemplate> GetMostUsedTemplates(int count = 10);

    /// <summary>
    /// 获取所有标签
    /// </summary>
    /// <returns>标签列表</returns>
    List<string> GetAllTags();
}