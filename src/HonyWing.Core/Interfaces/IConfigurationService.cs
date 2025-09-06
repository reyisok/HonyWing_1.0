namespace HonyWing.Core.Interfaces;

/// <summary>
/// 配置管理服务接口
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// 获取配置值
    /// </summary>
    /// <typeparam name="T">配置值类型</typeparam>
    /// <param name="key">配置键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>配置值</returns>
    T GetValue<T>(string key, T defaultValue = default!);

    /// <summary>
    /// 设置配置值
    /// </summary>
    /// <typeparam name="T">配置值类型</typeparam>
    /// <param name="key">配置键</param>
    /// <param name="value">配置值</param>
    void SetValue<T>(string key, T value);

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// 重置为默认配置
    /// </summary>
    void ResetToDefault();

    /// <summary>
    /// 检查配置键是否存在
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否存在</returns>
    bool ContainsKey(string key);

    /// <summary>
    /// 删除配置项
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否删除成功</returns>
    bool RemoveKey(string key);

    /// <summary>
    /// 获取所有配置键
    /// </summary>
    /// <returns>配置键列表</returns>
    IEnumerable<string> GetAllKeys();

    /// <summary>
    /// 配置变更事件
    /// </summary>
    event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    /// <summary>
    /// 导出配置到指定文件
    /// </summary>
    /// <param name="filePath">导出文件路径</param>
    /// <param name="includeVersion">是否包含版本信息</param>
    /// <returns>导出任务</returns>
    Task ExportConfigurationAsync(string filePath, bool includeVersion = true);

    /// <summary>
    /// 从指定文件导入配置
    /// </summary>
    /// <param name="filePath">导入文件路径</param>
    /// <param name="validatePaths">是否验证路径有效性</param>
    /// <returns>导入结果</returns>
    Task<ConfigurationImportResult> ImportConfigurationAsync(string filePath, bool validatePaths = true);

    /// <summary>
    /// 获取配置文件版本信息
    /// </summary>
    /// <param name="filePath">配置文件路径</param>
    /// <returns>版本信息</returns>
    Task<ConfigurationVersionInfo?> GetConfigurationVersionAsync(string filePath);

    /// <summary>
    /// 验证配置文件兼容性
    /// </summary>
    /// <param name="filePath">配置文件路径</param>
    /// <returns>兼容性检查结果</returns>
    Task<ConfigurationCompatibilityResult> ValidateCompatibilityAsync(string filePath);

    /// <summary>
    /// 获取当前配置的摘要信息
    /// </summary>
    /// <returns>配置摘要</returns>
    ConfigurationSummary GetConfigurationSummary();
}

/// <summary>
/// 配置变更事件参数
/// </summary>
public class ConfigurationChangedEventArgs : EventArgs
{
    /// <summary>
    /// 变更的配置键
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 旧值
    /// </summary>
    public object? OldValue { get; set; }

    /// <summary>
    /// 新值
    /// </summary>
    public object? NewValue { get; set; }

    /// <summary>
    /// 变更时间
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// 配置导入结果
/// </summary>
 public class ConfigurationImportResult
{
    /// <summary>
    /// 导入是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 导入的配置项数量
    /// </summary>
    public int ImportedCount { get; set; }

    /// <summary>
    /// 跳过的配置项数量
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// 失效的路径列表
    /// </summary>
    public List<string> InvalidPaths { get; set; } = new();

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 警告消息列表
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// 配置版本信息
/// </summary>
public class ConfigurationVersionInfo
{
    /// <summary>
    /// 软件版本
    /// </summary>
    public string SoftwareVersion { get; set; } = string.Empty;

    /// <summary>
    /// 配置文件版本
    /// </summary>
    public string ConfigVersion { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 导出时间
    /// </summary>
    public DateTime ExportedAt { get; set; }

    /// <summary>
    /// 配置项数量
    /// </summary>
    public int ConfigCount { get; set; }
}

/// <summary>
/// 配置兼容性检查结果
/// </summary>
public class ConfigurationCompatibilityResult
{
    /// <summary>
    /// 是否兼容
    /// </summary>
    public bool IsCompatible { get; set; }

    /// <summary>
    /// 兼容性级别
    /// </summary>
    public CompatibilityLevel Level { get; set; }

    /// <summary>
    /// 不兼容的配置项
    /// </summary>
    public List<string> IncompatibleKeys { get; set; } = new();

    /// <summary>
    /// 兼容性消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 建议操作
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// 兼容性级别
/// </summary>
public enum CompatibilityLevel
{
    /// <summary>
    /// 完全兼容
    /// </summary>
    FullyCompatible,

    /// <summary>
    /// 部分兼容
    /// </summary>
    PartiallyCompatible,

    /// <summary>
    /// 不兼容
    /// </summary>
    Incompatible,

    /// <summary>
    /// 版本过旧
    /// </summary>
    VersionTooOld,

    /// <summary>
    /// 版本过新
    /// </summary>
    VersionTooNew
}

/// <summary>
/// 配置摘要信息
/// </summary>
public class ConfigurationSummary
{
    /// <summary>
    /// 配置项总数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 分类统计
    /// </summary>
    public Dictionary<string, int> CategoryCounts { get; set; } = new();

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// 配置文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 是否有未保存的更改
    /// </summary>
    public bool HasUnsavedChanges { get; set; }
}