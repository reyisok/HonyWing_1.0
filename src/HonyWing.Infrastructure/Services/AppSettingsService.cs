using System;
using System.IO;
using System.Text.Json;
using HonyWing.Core.Interfaces;
using HonyWing.Core.Models;
using Microsoft.Extensions.Logging;

namespace HonyWing.Infrastructure.Services
{
    /// <summary>
    /// 应用程序设置服务实现
    /// @author: Mr.Rey Copyright © 2025
    /// @created: 2025-01-11 15:46:00
    /// @version: 1.0.0
    /// </summary>
    public class AppSettingsService : IAppSettingsService
    {
        private readonly ILogger<AppSettingsService> _logger;
        private readonly IPathService _pathService;
        private readonly string _settingsFilePath;
        private readonly object _fileLock = new object();

        public AppSettingsService(ILogger<AppSettingsService> logger, IPathService pathService)
        {
            _logger = logger;
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            
            // 使用PathService提供的用户配置路径
            var userConfigPath = _pathService.UserConfigPath;
            Directory.CreateDirectory(userConfigPath);
            _settingsFilePath = Path.Combine(userConfigPath, "appsettings.json");
        }

        public async Task<AppSettings> LoadAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    _logger.LogInformation("设置文件不存在，返回默认设置: {FilePath}", _settingsFilePath);
                    var defaultSettings = GetDefaultSettings();
                    await SaveAsync(defaultSettings);
                    return defaultSettings;
                }

                string json;
                lock (_fileLock)
                {
                    json = File.ReadAllText(_settingsFilePath);
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("设置文件为空，返回默认设置");
                    return GetDefaultSettings();
                }

                var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                });

                if (settings == null)
                {
                    _logger.LogWarning("反序列化设置文件失败，返回默认设置");
                    return GetDefaultSettings();
                }

                _logger.LogInformation("成功加载应用程序设置: {FilePath}", _settingsFilePath);
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载应用程序设置失败: {FilePath}", _settingsFilePath);
                return GetDefaultSettings();
            }
        }

        public async Task SaveAsync(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await Task.Run(() =>
                {
                    lock (_fileLock)
                    {
                        File.WriteAllText(_settingsFilePath, json);
                    }
                });

                _logger.LogInformation("成功保存应用程序设置: {FilePath}", _settingsFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存应用程序设置失败: {FilePath}", _settingsFilePath);
                throw;
            }
        }

        public AppSettings GetDefaultSettings()
        {
            return new AppSettings();
        }
    }
}
