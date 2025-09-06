using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HonyWing.Core.Interfaces;
using HonyWing.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace HonyWing.Infrastructure.Services;

/// <summary>
/// 图像模板服务实现
/// @author: Mr.Rey Copyright © 2025
/// @created: 2025-01-27 17:15:00
/// @version: 1.0.0
/// </summary>
public class ImageTemplateService : IImageTemplateService
{
    private readonly ILogger<ImageTemplateService> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly IPathService _pathService;
    private readonly ObservableCollection<ImageTemplate> _templates;
    private readonly string _templatesDirectory;
    private readonly string _templatesIndexFile;
    private bool _disposed = false;

    public ObservableCollection<ImageTemplate> Templates => _templates;

    public event EventHandler<ImageTemplate>? TemplateAdded;
    public event EventHandler<ImageTemplate>? TemplateUpdated;
    public event EventHandler<string>? TemplateRemoved;
    public event EventHandler? TemplatesCleared;

    public ImageTemplateService(
        ILogger<ImageTemplateService> logger,
        IConfigurationService configurationService,
        IPathService pathService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
        _templates = new ObservableCollection<ImageTemplate>();
        
        // 使用PathService提供的模板目录
        _templatesDirectory = _pathService.TemplatesPath;
        _templatesIndexFile = Path.Combine(_templatesDirectory, "templates.json");
        
        _logger.LogInformation("图像模板服务已初始化，模板目录: {TemplatesDirectory}", _templatesDirectory);
        
        // 确保目录存在
        EnsureDirectoryExists();
        
        // 加载现有模板
        _ = LoadTemplatesAsync();
    }



    private void EnsureDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(_templatesDirectory))
            {
                Directory.CreateDirectory(_templatesDirectory);
                _logger.LogInformation("创建模板目录: {Directory}", _templatesDirectory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建模板目录失败: {Directory}", _templatesDirectory);
            throw;
        }
    }

    public async Task<ImageTemplate> CreateTemplateAsync(string name, string imagePath, string? description = null, IEnumerable<string>? tags = null)
    {
        try
        {
            _logger.LogDebug("开始创建图像模板: {Name}, 图像路径: {ImagePath}", name, imagePath);
            
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("模板名称不能为空", nameof(name));
            
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                throw new ArgumentException("图像文件不存在", nameof(imagePath));

            // 检查名称是否已存在
            if (_templates.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("模板名称已存在: {Name}", name);
                throw new InvalidOperationException($"模板名称 '{name}' 已存在");
            }

            // 复制图像文件到模板目录
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imagePath)}";
            var targetPath = Path.Combine(_templatesDirectory, fileName);
            File.Copy(imagePath, targetPath, false);
            
            var template = new ImageTemplate
            {
                Id = Guid.NewGuid(),
                Name = name,
                ImagePath = targetPath,
                Description = description ?? string.Empty,
                Tags = tags?.ToList() ?? new List<string>(),
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
                UsageCount = 0
            };

            _templates.Add(template);
            await SaveTemplatesAsync();
            
            _logger.LogInformation("成功创建图像模板: {Name}, ID: {Id}", name, template.Id);
            TemplateAdded?.Invoke(this, template);
            
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建图像模板失败: {Name}", name);
            throw;
        }
    }

    public async Task<bool> UpdateTemplateAsync(ImageTemplate template)
    {
        try
        {
            _logger.LogDebug("开始更新图像模板: {Id}", template.Id);
            
            var existingTemplate = _templates.FirstOrDefault(t => t.Id == template.Id);
            if (existingTemplate == null)
            {
                _logger.LogWarning("未找到要更新的模板: {Id}", template.Id);
                return false;
            }

            var index = _templates.IndexOf(existingTemplate);
            template.ModifiedAt = DateTime.Now;
            _templates[index] = template;
            
            await SaveTemplatesAsync();
            
            _logger.LogInformation("成功更新图像模板: {Name}, ID: {Id}", template.Name, template.Id);
            TemplateUpdated?.Invoke(this, template);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新图像模板失败: {Id}", template.Id);
            throw;
        }
    }

    public async Task<bool> DeleteTemplateAsync(string id)
    {
        try
        {
            _logger.LogDebug("开始删除图像模板: {Id}", id);
            
            if (!Guid.TryParse(id, out var guidId)) return false;
            var template = _templates.FirstOrDefault(t => t.Id == guidId);
            if (template == null)
            {
                _logger.LogWarning("未找到要删除的模板: {Id}", id);
                return false;
            }

            var templateName = template.Name;
            
            // 删除图像文件
            try
            {
                if (File.Exists(template.ImagePath))
                {
                    File.Delete(template.ImagePath);
                    _logger.LogDebug("已删除模板图像文件: {ImagePath}", template.ImagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "删除模板图像文件失败: {ImagePath}", template.ImagePath);
            }

            _templates.Remove(template);
            await SaveTemplatesAsync();
            
            _logger.LogInformation("成功删除图像模板: {Name}, ID: {Id}", templateName, id);
            TemplateRemoved?.Invoke(this, id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除图像模板失败: {Id}", id);
            throw;
        }
    }

    public Task<ImageTemplate?> GetTemplateAsync(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var guidId)) return Task.FromResult<ImageTemplate?>(null);
            var template = _templates.FirstOrDefault(t => t.Id == guidId);
            if (template != null)
            {
                _logger.LogDebug("获取图像模板: {Name}, ID: {Id}", template.Name, id);
            }
            else
            {
                _logger.LogDebug("未找到图像模板: {Id}", id);
            }
            return Task.FromResult(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取图像模板失败: {Id}", id);
            throw;
        }
    }

    public Task<IEnumerable<ImageTemplate>> GetAllTemplatesAsync()
    {
        try
        {
            _logger.LogDebug("获取所有图像模板，共 {Count} 个", _templates.Count);
            return Task.FromResult<IEnumerable<ImageTemplate>>(_templates.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有图像模板失败");
            throw;
        }
    }

    public Task<IEnumerable<ImageTemplate>> SearchTemplatesAsync(string? keyword = null, IEnumerable<string>? tags = null)
    {
        try
        {
            _logger.LogDebug("搜索图像模板，关键字: {Keyword}, 标签: {Tags}", keyword ?? "<无>", tags != null ? string.Join(",", tags) : "<无>");
            
            var query = _templates.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(t => 
                    t.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            if (tags != null && tags.Any())
            {
                var tagList = tags.ToList();
                query = query.Where(t => tagList.Any(tag => t.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
            }

            var results = query.ToList();
            _logger.LogDebug("搜索结果: {Count} 个模板", results.Count);
            
            return Task.FromResult<IEnumerable<ImageTemplate>>(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索图像模板失败");
            throw;
        }
    }

    public async Task<bool> ImportTemplatesAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("开始导入模板文件: {FilePath}", filePath);
            
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("导入文件不存在: {FilePath}", filePath);
                return false;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var importedTemplates = JsonSerializer.Deserialize<List<ImageTemplate>>(json);
            
            if (importedTemplates == null || !importedTemplates.Any())
            {
                _logger.LogWarning("导入文件中没有有效的模板数据");
                return false;
            }

            var importedCount = 0;
            foreach (var template in importedTemplates)
            {
                try
                {
                    // 检查名称冲突
                    if (_templates.Any(t => t.Name.Equals(template.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogWarning("跳过重复模板: {Name}", template.Name);
                        continue;
                    }

                    // 检查图像文件是否存在
                    if (!File.Exists(template.ImagePath))
                    {
                        _logger.LogWarning("跳过缺失图像文件的模板: {Name}, 路径: {ImagePath}", template.Name, template.ImagePath);
                        continue;
                    }

                    template.Id = Guid.NewGuid();
                    template.CreatedAt = DateTime.Now;
                    template.ModifiedAt = DateTime.Now;
                    
                    _templates.Add(template);
                    importedCount++;
                    
                    TemplateAdded?.Invoke(this, template);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "导入模板失败: {Name}", template.Name);
                }
            }

            if (importedCount > 0)
            {
                await SaveTemplatesAsync();
            }
            
            _logger.LogInformation("模板导入完成，成功导入 {ImportedCount} 个模板", importedCount);
            return importedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导入模板失败: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> ExportTemplatesAsync(string filePath, IEnumerable<string>? templateIds = null)
    {
        try
        {
            _logger.LogInformation("开始导出模板到文件: {FilePath}", filePath);
            
            var templatesToExport = templateIds != null
                ? _templates.Where(t => templateIds.Contains(t.Id.ToString())).ToList()
                : _templates.ToList();

            if (!templatesToExport.Any())
            {
                _logger.LogWarning("没有要导出的模板");
                return false;
            }

            var json = JsonSerializer.Serialize(templatesToExport, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);
            
            _logger.LogInformation("模板导出完成，导出 {Count} 个模板到: {FilePath}", templatesToExport.Count, filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出模板失败: {FilePath}", filePath);
            return false;
        }
    }

    public async Task ClearTemplatesAsync()
    {
        try
        {
            _logger.LogInformation("开始清空所有模板，当前模板数量: {Count}", _templates.Count);
            
            // 删除所有图像文件
            foreach (var template in _templates.ToList())
            {
                try
                {
                    if (File.Exists(template.ImagePath))
                    {
                        File.Delete(template.ImagePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "删除模板图像文件失败: {ImagePath}", template.ImagePath);
                }
            }

            _templates.Clear();
            await SaveTemplatesAsync();
            
            _logger.LogInformation("所有模板已清空");
            TemplatesCleared?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空模板失败");
            throw;
        }
    }

    public async Task IncrementUsageCountAsync(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var guidId)) return;
            var template = _templates.FirstOrDefault(t => t.Id == guidId);
            if (template != null)
            {
                template.UsageCount++;
                template.ModifiedAt = DateTime.Now;
                await SaveTemplatesAsync();
                
                _logger.LogDebug("模板使用次数已更新: {Name}, 使用次数: {UsageCount}", template.Name, template.UsageCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新模板使用次数失败: {Id}", id);
        }
    }

    public Task<IEnumerable<string>> GetAllTagsAsync()
    {
        try
        {
            var allTags = _templates
                .SelectMany(t => t.Tags)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag)
                .ToList();
            
            _logger.LogDebug("获取所有标签，共 {Count} 个", allTags.Count);
            return Task.FromResult<IEnumerable<string>>(allTags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有标签失败");
            throw;
        }
    }

    private async Task LoadTemplatesAsync()
    {
        try
        {
            if (!File.Exists(_templatesIndexFile))
            {
                _logger.LogInformation("模板索引文件不存在，将创建新的索引文件: {IndexFile}", _templatesIndexFile);
                return;
            }

            var json = await File.ReadAllTextAsync(_templatesIndexFile);
            var templates = JsonSerializer.Deserialize<List<ImageTemplate>>(json);
            
            if (templates != null)
            {
                foreach (var template in templates)
                {
                    // 验证图像文件是否存在
                    if (File.Exists(template.ImagePath))
                    {
                        _templates.Add(template);
                    }
                    else
                    {
                        _logger.LogWarning("模板图像文件不存在，跳过加载: {Name}, 路径: {ImagePath}", template.Name, template.ImagePath);
                    }
                }
            }
            
            _logger.LogInformation("成功加载 {Count} 个图像模板", _templates.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载模板失败: {IndexFile}", _templatesIndexFile);
        }
    }

    private async Task SaveTemplatesAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_templates.ToList(), new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_templatesIndexFile, json);
            _logger.LogDebug("模板索引已保存: {IndexFile}", _templatesIndexFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存模板索引失败: {IndexFile}", _templatesIndexFile);
            throw;
        }
    }

    public void UpdateUsageStatistics(string templateId)
    {
        try
        {
            if (!Guid.TryParse(templateId, out var guidId)) return;
            var template = _templates.FirstOrDefault(t => t.Id == guidId);
            if (template != null)
            {
                template.UsageCount++;
                template.LastUsedAt = DateTime.Now;
                template.ModifiedAt = DateTime.Now;
                
                _logger.LogDebug("模板使用统计已更新: {Name}, 使用次数: {UsageCount}", template.Name, template.UsageCount);
                
                // 异步保存，不阻塞调用
                _ = Task.Run(async () => await SaveTemplatesAsync());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新模板使用统计失败: {TemplateId}", templateId);
        }
    }

    public List<ImageTemplate> GetRecentlyUsedTemplates(int count = 10)
    {
        try
        {
            return _templates
                .Where(t => t.LastUsedAt.HasValue)
                .OrderByDescending(t => t.LastUsedAt)
                .Take(count)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最近使用的模板失败");
            return new List<ImageTemplate>();
        }
    }

    public List<ImageTemplate> GetMostUsedTemplates(int count = 10)
    {
        try
        {
            return _templates
                .OrderByDescending(t => t.UsageCount)
                .Take(count)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最常使用的模板失败");
            return new List<ImageTemplate>();
        }
    }

    public List<string> GetAllTags()
    {
        try
        {
            return _templates
                .SelectMany(t => t.Tags ?? new List<string>())
                .Distinct()
                .OrderBy(tag => tag)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有标签失败");
            return new List<string>();
        }
    }

    public ImageTemplate? GetTemplate(string templateId)
    {
        try
        {
            if (Guid.TryParse(templateId, out var guidId))
             {
                 return _templates.FirstOrDefault(t => t.Id == guidId);
             }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取模板失败: {TemplateId}", templateId);
            return null;
        }
    }

    public ImageTemplate? GetTemplateByName(string name)
    {
        try
        {
            return _templates.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据名称获取模板失败: {Name}", name);
            return null;
        }
    }

    public List<ImageTemplate> GetTemplatesByTag(string tag)
    {
        try
        {
            return _templates
                .Where(t => t.Tags != null && t.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据标签获取模板失败: {Tag}", tag);
            return new List<ImageTemplate>();
        }
    }

    public List<ImageTemplate> SearchTemplates(string keyword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _templates.ToList();

            var lowerKeyword = keyword.ToLowerInvariant();
            return _templates
                .Where(t => 
                    t.Name.ToLowerInvariant().Contains(lowerKeyword) ||
                    (t.Description != null && t.Description.ToLowerInvariant().Contains(lowerKeyword)) ||
                    (t.Tags != null && t.Tags.Any(tag => tag.ToLowerInvariant().Contains(lowerKeyword))))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索模板失败: {Keyword}", keyword);
            return new List<ImageTemplate>();
        }
    }

    public async Task<ImageTemplate?> CreateTemplateAsync(string name, string imagePath, string? description = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("模板名称不能为空");
                return null;
            }

            if (!File.Exists(imagePath))
            {
                _logger.LogWarning("图像文件不存在: {ImagePath}", imagePath);
                return null;
            }

            var template = new ImageTemplate
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description ?? string.Empty,
                ImagePath = imagePath,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };

            _templates.Add(template);
            await SaveTemplatesAsync();

            _logger.LogInformation("成功创建图像模板: {Name}, ID: {Id}", name, template.Id);
            TemplateAdded?.Invoke(this, template);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建图像模板失败: {Name}", name);
            return null;
        }
    }

    public Task<ImageTemplate?> CreateTemplateFromClipboardAsync(string name, string? description = null)
    {
        try
        {
            _logger.LogInformation("从剪贴板创建模板功能暂未实现: {Name}", name);
            return Task.FromResult<ImageTemplate?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从剪贴板创建模板失败: {Name}", name);
            return Task.FromResult<ImageTemplate?>(null);
        }
    }



    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                _logger.LogInformation("图像模板服务正在释放资源");
                _templates.Clear();
                _disposed = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放图像模板服务资源时发生错误");
            }
        }
    }
}