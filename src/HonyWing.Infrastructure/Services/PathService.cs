using System;
using System.IO;
using System.Reflection;
using HonyWing.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace HonyWing.Infrastructure.Services;

/// <summary>
/// 路径管理服务 - 统一管理项目中的所有路径
/// @author: Mr.Rey Copyright © 2025
/// @created: 2025-01-27 17:30:00
/// @version: 1.0.0
/// </summary>
public class PathService : IPathService
{
    private readonly ILogger<PathService> _logger;
    private readonly string _projectRoot;
    private readonly string _applicationDataPath;
    
    public string ProjectRoot => _projectRoot;
    public string SourceRoot => Path.Combine(_projectRoot, "src");
    public string AssetsRoot => Path.Combine(_projectRoot, "assets");
    public string ConfigRoot => Path.Combine(_projectRoot, "config");
    public string LogsRoot => Path.Combine(_projectRoot, "logs");
    public string DocsRoot => Path.Combine(_projectRoot, "docs");
    public string ToolsRoot => Path.Combine(_projectRoot, "tools");
    public string BuildRoot => Path.Combine(_projectRoot, "build");
    
    // 应用数据目录（用户配置等）
    public string ApplicationDataRoot => _applicationDataPath;
    public string UserConfigPath => Path.Combine(_applicationDataPath, "config");
    public string UserTemplatesPath => Path.Combine(_applicationDataPath, "templates");
    public string UserLogsPath => Path.Combine(_applicationDataPath, "logs");
    
    // 资源子目录
    public string IconsPath => Path.Combine(AssetsRoot, "icons");
    public string ImagesPath => Path.Combine(AssetsRoot, "images");
    public string TemplatesPath => Path.Combine(AssetsRoot, "templates");
    
    public PathService(ILogger<PathService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // 确定项目根目录
        _projectRoot = DetermineProjectRoot();
        
        // 设置应用数据目录
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _applicationDataPath = Path.Combine(appDataPath, "HonyWing");
        
        _logger.LogInformation("路径服务已初始化，项目根目录: {ProjectRoot}", _projectRoot);
        _logger.LogInformation("应用数据目录: {ApplicationDataRoot}", _applicationDataPath);
        
        // 确保必要的目录存在
        EnsureDirectoriesExist();
    }
    
    /// <summary>
    /// 确定项目根目录
    /// </summary>
    private string DetermineProjectRoot()
    {
        try
        {
            // 方法1：从当前工作目录查找
            var currentDir = Directory.GetCurrentDirectory();
            var projectRoot = FindProjectRootFromPath(currentDir);
            if (projectRoot != null)
            {
                _logger.LogDebug("从当前工作目录找到项目根目录: {ProjectRoot}", projectRoot);
                return projectRoot;
            }
            
            // 方法2：从程序集位置查找
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(assemblyLocation))
            {
                var assemblyDir = Path.GetDirectoryName(assemblyLocation);
                if (!string.IsNullOrEmpty(assemblyDir))
                {
                    projectRoot = FindProjectRootFromPath(assemblyDir);
                    if (projectRoot != null)
                    {
                        _logger.LogDebug("从程序集位置找到项目根目录: {ProjectRoot}", projectRoot);
                        return projectRoot;
                    }
                }
            }
            
            // 方法3：从入口程序集位置查找
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                var entryLocation = entryAssembly.Location;
                if (!string.IsNullOrEmpty(entryLocation))
                {
                    var entryDir = Path.GetDirectoryName(entryLocation);
                    if (!string.IsNullOrEmpty(entryDir))
                    {
                        projectRoot = FindProjectRootFromPath(entryDir);
                        if (projectRoot != null)
                        {
                            _logger.LogDebug("从入口程序集位置找到项目根目录: {ProjectRoot}", projectRoot);
                            return projectRoot;
                        }
                    }
                }
            }
            
            // 如果都找不到，使用当前目录作为备选
            _logger.LogWarning("无法确定项目根目录，使用当前工作目录: {CurrentDirectory}", currentDir);
            return currentDir;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "确定项目根目录时发生错误");
            return Directory.GetCurrentDirectory();
        }
    }
    
    /// <summary>
    /// 从指定路径向上查找项目根目录
    /// </summary>
    private string? FindProjectRootFromPath(string startPath)
    {
        var currentPath = startPath;
        
        while (!string.IsNullOrEmpty(currentPath))
        {
            // 检查是否包含src目录
            var srcPath = Path.Combine(currentPath, "src");
            if (Directory.Exists(srcPath))
            {
                // 进一步验证是否为HonyWing项目根目录
                var solutionFile = Path.Combine(currentPath, "HonyWing.sln");
                var globalJsonFile = Path.Combine(currentPath, "global.json");
                
                if (File.Exists(solutionFile) || File.Exists(globalJsonFile))
                {
                    return currentPath;
                }
            }
            
            // 向上一级目录查找
            var parentPath = Path.GetDirectoryName(currentPath);
            if (parentPath == currentPath) // 已到达根目录
                break;
                
            currentPath = parentPath;
        }
        
        return null;
    }
    
    /// <summary>
    /// 确保必要的目录存在
    /// </summary>
    private void EnsureDirectoriesExist()
    {
        try
        {
            var directories = new[]
            {
                AssetsRoot,
                ConfigRoot,
                LogsRoot,
                ApplicationDataRoot,
                UserConfigPath,
                UserTemplatesPath,
                UserLogsPath,
                IconsPath,
                ImagesPath,
                TemplatesPath
            };
            
            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogDebug("创建目录: {Directory}", directory);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建必要目录时发生错误");
        }
    }
    
    /// <summary>
    /// 获取相对于项目根目录的路径
    /// </summary>
    public string GetRelativePath(string fullPath)
    {
        try
        {
            return Path.GetRelativePath(_projectRoot, fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取相对路径失败: {FullPath}", fullPath);
            return fullPath;
        }
    }
    
    /// <summary>
    /// 获取相对于项目根目录的绝对路径
    /// </summary>
    public string GetAbsolutePath(string relativePath)
    {
        try
        {
            if (Path.IsPathRooted(relativePath))
                return relativePath;
                
            return Path.Combine(_projectRoot, relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取绝对路径失败: {RelativePath}", relativePath);
            return relativePath;
        }
    }
    
    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    public string GetConfigFilePath(string fileName)
    {
        // 优先使用用户配置目录，如果不存在则使用项目配置目录
        var userConfigFile = Path.Combine(UserConfigPath, fileName);
        if (File.Exists(userConfigFile))
        {
            return userConfigFile;
        }
        
        return Path.Combine(ConfigRoot, fileName);
    }
    
    /// <summary>
    /// 获取日志文件路径
    /// </summary>
    public string GetLogFilePath(string fileName)
    {
        return Path.Combine(UserLogsPath, fileName);
    }
    
    /// <summary>
    /// 获取模板文件路径
    /// </summary>
    public string GetTemplateFilePath(string fileName)
    {
        // 优先使用用户模板目录，如果不存在则使用项目模板目录
        var userTemplateFile = Path.Combine(UserTemplatesPath, fileName);
        if (File.Exists(userTemplateFile))
        {
            return userTemplateFile;
        }
        
        return Path.Combine(TemplatesPath, fileName);
    }
    
    /// <summary>
    /// 获取资源文件路径
    /// </summary>
    public string GetAssetFilePath(string relativePath)
    {
        return Path.Combine(AssetsRoot, relativePath);
    }
    
    /// <summary>
    /// 验证路径是否在项目范围内
    /// </summary>
    public bool IsPathInProject(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            var projectFullPath = Path.GetFullPath(_projectRoot);
            
            return fullPath.StartsWith(projectFullPath, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "验证路径时发生错误: {Path}", path);
            return false;
        }
    }
    
    /// <summary>
    /// 清理临时文件
    /// </summary>
    public void CleanupTempFiles()
    {
        try
        {
            var tempPath = Path.GetTempPath();
            var honywingTempPath = Path.Combine(tempPath, "HonyWing");
            
            if (Directory.Exists(honywingTempPath))
            {
                Directory.Delete(honywingTempPath, true);
                _logger.LogInformation("已清理临时文件: {TempPath}", honywingTempPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "清理临时文件时发生错误");
        }
    }
}