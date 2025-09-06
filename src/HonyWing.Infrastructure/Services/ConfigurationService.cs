using HonyWing.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace HonyWing.Infrastructure.Services;

/// <summary>
/// 配置管理服务实现
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly ConcurrentDictionary<string, object> _configuration;
    private readonly string _configFilePath;
    private readonly object _fileLock = new();
    private bool _hasUnsavedChanges = false;
    private DateTime _lastModified = DateTime.Now;

    private const string CONFIG_VERSION = "1.0.0";
    private static readonly string SOFTWARE_VERSION = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
        _configuration = new ConcurrentDictionary<string, object>();

        // 配置文件路径
        var configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HonyWing");
        Directory.CreateDirectory(configDirectory);
        _configFilePath = Path.Combine(configDirectory, "config.json");

        // 初始化时加载配置
        _ = LoadAsync();
    }

    public T GetValue<T>(string key, T defaultValue = default!)
    {
        try
        {
            if (_configuration.TryGetValue(key, out var value))
            {
                if (value is T directValue)
                {
                    return directValue;
                }

                // 尝试类型转换
                if (value != null)
                {
                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "配置值类型转换失败, Key: {Key}, Value: {Value}, TargetType: {TargetType}",
                            key, value, typeof(T).Name);
                    }
                }
            }

            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取配置值失败, Key: {Key}", key);
            return defaultValue;
        }
    }

    public void SetValue<T>(string key, T value)
    {
        try
        {
            var oldValue = _configuration.TryGetValue(key, out var existing) ? existing : null;

            _configuration.AddOrUpdate(key, value!, (k, v) => value!);

            // 标记有未保存的更改
            _hasUnsavedChanges = true;
            _lastModified = DateTime.Now;

            // 触发配置变更事件
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
            {
                Key = key,
                OldValue = oldValue,
                NewValue = value
            });

            _logger.LogDebug("配置值已更新, Key: {Key}, NewValue: {NewValue}", key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置配置值失败, Key: {Key}, Value: {Value}", key, value);
            throw;
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_configuration.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Formatting.Indented);

            await Task.Run(() =>
            {
                lock (_fileLock)
                {
                    File.WriteAllText(_configFilePath, json);
                }
            });

            // 重置未保存更改标志
            _hasUnsavedChanges = false;

            _logger.LogInformation("配置已保存到文件: {FilePath}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存配置文件失败: {FilePath}", _configFilePath);
            throw;
        }
    }

    public async Task LoadAsync()
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                _logger.LogInformation("配置文件不存在，使用默认配置: {FilePath}", _configFilePath);
                LoadDefaultConfiguration();
                await SaveAsync();
                return;
            }

            lock (_fileLock)
            {
                var json = File.ReadAllText(_configFilePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (config != null)
                    {
                        _configuration.Clear();
                        foreach (var kvp in config)
                        {
                            _configuration.TryAdd(kvp.Key, kvp.Value);
                        }
                    }
                }
            }

            _logger.LogInformation("从文件加载配置: {FilePath}, 配置项数量: {Count}", _configFilePath, _configuration.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载配置文件失败: {FilePath}", _configFilePath);
            LoadDefaultConfiguration();
        }
    }

    public void ResetToDefault()
    {
        try
        {
            _configuration.Clear();
            LoadDefaultConfiguration();

            _logger.LogInformation("配置已重置为默认值");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重置配置失败");
            throw;
        }
    }

    public bool ContainsKey(string key)
    {
        return _configuration.ContainsKey(key);
    }

    public bool RemoveKey(string key)
    {
        try
        {
            var removed = _configuration.TryRemove(key, out var oldValue);
            if (removed)
            {
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
                {
                    Key = key,
                    OldValue = oldValue,
                    NewValue = null
                });

                _logger.LogDebug("配置项已移除, Key: {Key}", key);
            }
            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除配置项失败, Key: {Key}", key);
            return false;
        }
    }

    public IEnumerable<string> GetAllKeys()
    {
        return _configuration.Keys.ToList();
    }

    private void LoadDefaultConfiguration()
    {
        // 通用设置


        SetValue("General.CheckForUpdates", true);

        // 图像匹配设置
        SetValue("ImageMatching.DefaultThreshold", 0.8);
        SetValue("ImageMatching.TimeoutMs", 5000);
        SetValue("ImageMatching.SearchIntervalMs", 100);
        SetValue("ImageMatching.MaxSearchAttempts", 50);
        SetValue("ImageMatching.EnableMultiScale", true);
        SetValue("ImageMatching.ScaleRange", new[] { 0.8, 1.2 });

        // 鼠标操作设置
        SetValue("Mouse.ClickDelayMs", 100);
        SetValue("Mouse.MoveSpeed", 2.0);
        SetValue("Mouse.EnableSmoothMove", true);
        SetValue("Mouse.DoubleClickIntervalMs", 200);
        SetValue("Mouse.DragDelayMs", 50);

        // 界面设置

        SetValue("UI.AccentColor", "#0078D4");
        SetValue("UI.WindowLocation", new { X = 100, Y = 100 });
        SetValue("UI.WindowSize", new { Width = 1200, Height = 800 });
        SetValue("UI.WindowState", "Normal");
        SetValue("UI.ShowGridLines", true);
        SetValue("UI.ShowCoordinates", true);

        // 日志设置
        SetValue("Logging.LogLevel", "Information");
        SetValue("Logging.EnableFileLogging", true);
        SetValue("Logging.EnableConsoleLogging", true);
        SetValue("Logging.MaxFileSizeMB", 10);
        SetValue("Logging.RetainedFileCount", 7);
        SetValue("Logging.OutputTemplate", "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

        _logger.LogInformation("默认配置已加载");
    }

    #region 配置导出导入功能

    /// <summary>
    /// 导出配置到指定文件
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-13 15:30:00
    /// @version: 1.0.0
    /// </summary>
    public async Task ExportConfigurationAsync(string filePath, bool includeVersion = true)
    {
        try
        {
            var exportData = new Dictionary<string, object>();

            // 添加版本信息
            if (includeVersion)
            {
                exportData["_metadata"] = new
                {
                    SoftwareVersion = SOFTWARE_VERSION,
                    ConfigVersion = CONFIG_VERSION,
                    ExportedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    ConfigCount = _configuration.Count
                };
            }

            // 添加配置数据
            exportData["configuration"] = _configuration.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // 序列化并保存
            var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);

            _logger.LogInformation("配置已导出到文件: {FilePath}, 配置项数量: {Count}", filePath, _configuration.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出配置文件失败: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// 从指定文件导入配置
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-13 15:30:00
    /// @version: 1.0.0
    /// </summary>
    public async Task<ConfigurationImportResult> ImportConfigurationAsync(string filePath, bool validatePaths = true)
    {
        var result = new ConfigurationImportResult();

        try
        {
            if (!File.Exists(filePath))
            {
                result.ErrorMessage = "配置文件不存在";
                return result;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var importData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (importData == null)
            {
                result.ErrorMessage = "配置文件格式无效";
                return result;
            }

            // 检查兼容性
            var compatibilityResult = await ValidateCompatibilityAsync(filePath);
            if (!compatibilityResult.IsCompatible && compatibilityResult.Level == CompatibilityLevel.Incompatible)
            {
                result.ErrorMessage = compatibilityResult.Message;
                return result;
            }

            // 获取配置数据
            var configData = importData.ContainsKey("configuration")
                ? JsonConvert.DeserializeObject<Dictionary<string, object>>(importData["configuration"].ToString() ?? "{}")
                : importData;

            if (configData == null)
            {
                result.ErrorMessage = "Unable to parse configuration data";
                return result;
            }

            // 导入配置项
            foreach (var kvp in configData)
            {
                try
                {
                    // 跳过元数据
                    if (kvp.Key.StartsWith("_"))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    // 验证路径有效性（如果需要）
                    if (validatePaths && IsPathConfiguration(kvp.Key, kvp.Value))
                    {
                        if (!ValidatePathValue(kvp.Value))
                        {
                            result.InvalidPaths.Add($"{kvp.Key}: {kvp.Value}");
                            result.Warnings.Add($"Invalid path for configuration item '{kvp.Key}': {kvp.Value}");
                            result.SkippedCount++;
                            continue;
                        }
                    }

                    // 导入配置项
                    _configuration.AddOrUpdate(kvp.Key, kvp.Value, (k, v) => kvp.Value);
                    result.ImportedCount++;
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"Failed to import configuration item '{kvp.Key}': {ex.Message}");
                    result.SkippedCount++;
                }
            }

            // 标记有未保存的更改
            _hasUnsavedChanges = true;
            _lastModified = DateTime.Now;

            result.IsSuccess = true;
            _logger.LogInformation("配置导入完成: 已导入 {ImportedCount} 项, 跳过 {SkippedCount} 项",
                result.ImportedCount, result.SkippedCount);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "导入配置文件失败: {FilePath}", filePath);
        }

        return result;
    }

    /// <summary>
    /// 获取配置文件版本信息
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-13 15:30:00
    /// @version: 1.0.0
    /// </summary>
    public async Task<ConfigurationVersionInfo?> GetConfigurationVersionAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (data?.ContainsKey("_metadata") == true)
            {
                var metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(data["_metadata"].ToString() ?? "{}");
                if (metadata != null)
                {
                    return new ConfigurationVersionInfo
                    {
                        SoftwareVersion = metadata.GetValueOrDefault("SoftwareVersion", "").ToString() ?? "",
                        ConfigVersion = metadata.GetValueOrDefault("ConfigVersion", "").ToString() ?? "",
                        CreatedAt = DateTime.TryParse(metadata.GetValueOrDefault("CreatedAt", DateTime.MinValue).ToString(), out var created) ? created : DateTime.MinValue,
                        ExportedAt = DateTime.TryParse(metadata.GetValueOrDefault("ExportedAt", DateTime.MinValue).ToString(), out var exported) ? exported : DateTime.MinValue,
                        ConfigCount = int.TryParse(metadata.GetValueOrDefault("ConfigCount", 0).ToString(), out var count) ? count : 0
                    };
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取配置文件版本信息失败: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 验证配置文件兼容性
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-13 15:30:00
    /// @version: 1.0.0
    /// </summary>
    public async Task<ConfigurationCompatibilityResult> ValidateCompatibilityAsync(string filePath)
    {
        var result = new ConfigurationCompatibilityResult();

        try
        {
            var versionInfo = await GetConfigurationVersionAsync(filePath);

            if (versionInfo == null)
            {
                result.IsCompatible = true;
                result.Level = CompatibilityLevel.PartiallyCompatible;
                result.Message = "Configuration file lacks version information, will attempt compatible import";
                result.Recommendation = "Recommend re-exporting configuration file to include version information";
                return result;
            }

            // 检查配置文件版本兼容性
            var configVersionComparison = CompareVersions(versionInfo.ConfigVersion, CONFIG_VERSION);
            var softwareVersionComparison = CompareVersions(versionInfo.SoftwareVersion, SOFTWARE_VERSION);

            if (configVersionComparison == 0)
            {
                result.IsCompatible = true;
                result.Level = CompatibilityLevel.FullyCompatible;
                result.Message = "Configuration file is fully compatible";
            }
            else if (configVersionComparison < 0)
            {
                result.IsCompatible = true;
                result.Level = CompatibilityLevel.PartiallyCompatible;
                result.Message = "配置文件版本较旧，可能缺少一些新功能配置";
                result.Recommendation = "建议导入后检查并更新配置";
            }
            else
            {
                result.IsCompatible = false;
                result.Level = CompatibilityLevel.VersionTooNew;
                result.Message = "配置文件版本过新，当前软件版本不支持";
                result.Recommendation = "请升级软件到最新版本";
            }
        }
        catch (Exception ex)
        {
            result.IsCompatible = false;
            result.Level = CompatibilityLevel.Incompatible;
            result.Message = $"验证兼容性时发生错误: {ex.Message}";
            result.Recommendation = "请检查配置文件格式是否正确";
            _logger.LogError(ex, "验证配置文件兼容性失败: {FilePath}", filePath);
        }

        return result;
    }

    /// <summary>
    /// 获取当前配置的摘要信息
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-13 15:30:00
    /// @version: 1.0.0
    /// </summary>
    public ConfigurationSummary GetConfigurationSummary()
    {
        var summary = new ConfigurationSummary
        {
            TotalCount = _configuration.Count,
            LastModified = _lastModified,
            HasUnsavedChanges = _hasUnsavedChanges
        };

        // 计算配置文件大小
        try
        {
            if (File.Exists(_configFilePath))
            {
                var fileInfo = new FileInfo(_configFilePath);
                summary.FileSize = fileInfo.Length;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取配置文件大小失败");
        }

        // 统计各类别配置项数量
        var categories = new Dictionary<string, int>();
        foreach (var key in _configuration.Keys)
        {
            var category = key.Contains('.') ? key.Split('.')[0] : "Other";
            categories[category] = categories.GetValueOrDefault(category, 0) + 1;
        }
        summary.CategoryCounts = categories;

        return summary;
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 判断是否为路径相关配置
    /// </summary>
    private bool IsPathConfiguration(string key, object value)
    {
        if (value?.ToString() == null) return false;

        var lowerKey = key.ToLower();
        var valueStr = value.ToString()!;

        return (lowerKey.Contains("path") || lowerKey.Contains("file") || lowerKey.Contains("directory")) &&
               (valueStr.Contains("\\") || valueStr.Contains("/") || valueStr.Contains(":"));
    }

    /// <summary>
    /// 验证路径值的有效性
    /// </summary>
    private bool ValidatePathValue(object value)
    {
        if (value?.ToString() == null) return false;

        var path = value.ToString()!;
        try
        {
            // 检查路径格式是否有效
            Path.GetFullPath(path);

            // 检查文件或目录是否存在
            return File.Exists(path) || Directory.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 比较版本号
    /// </summary>
    /// <param name="version1">版本1</param>
    /// <param name="version2">版本2</param>
    /// <returns>-1: version1 < version2, 0: 相等, 1: version1 > version2</returns>
    private int CompareVersions(string version1, string version2)
    {
        try
        {
            var v1 = new Version(version1);
            var v2 = new Version(version2);
            return v1.CompareTo(v2);
        }
        catch
        {
            return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase);
        }
    }

    #endregion
}
