using System;

namespace HonyWing.Core.Interfaces;

/// <summary>
/// 路径管理服务接口
/// @author: Mr.Rey Copyright © 2025
/// @created: 2025-01-27 17:32:00
/// @version: 1.0.0
/// </summary>
public interface IPathService
{
    /// <summary>
    /// 项目根目录
    /// </summary>
    string ProjectRoot { get; }
    
    /// <summary>
    /// 源代码根目录
    /// </summary>
    string SourceRoot { get; }
    
    /// <summary>
    /// 资源文件根目录
    /// </summary>
    string AssetsRoot { get; }
    
    /// <summary>
    /// 配置文件根目录
    /// </summary>
    string ConfigRoot { get; }
    
    /// <summary>
    /// 日志文件根目录
    /// </summary>
    string LogsRoot { get; }
    
    /// <summary>
    /// 文档根目录
    /// </summary>
    string DocsRoot { get; }
    
    /// <summary>
    /// 工具根目录
    /// </summary>
    string ToolsRoot { get; }
    
    /// <summary>
    /// 构建输出根目录
    /// </summary>
    string BuildRoot { get; }
    
    /// <summary>
    /// 应用数据根目录（用户配置等）
    /// </summary>
    string ApplicationDataRoot { get; }
    
    /// <summary>
    /// 用户配置目录
    /// </summary>
    string UserConfigPath { get; }
    
    /// <summary>
    /// 用户模板目录
    /// </summary>
    string UserTemplatesPath { get; }
    
    /// <summary>
    /// 用户日志目录
    /// </summary>
    string UserLogsPath { get; }
    
    /// <summary>
    /// 图标文件目录
    /// </summary>
    string IconsPath { get; }
    
    /// <summary>
    /// 图片资源目录
    /// </summary>
    string ImagesPath { get; }
    
    /// <summary>
    /// 模板文件目录
    /// </summary>
    string TemplatesPath { get; }
    
    /// <summary>
    /// 获取相对于项目根目录的路径
    /// </summary>
    /// <param name="fullPath">完整路径</param>
    /// <returns>相对路径</returns>
    string GetRelativePath(string fullPath);
    
    /// <summary>
    /// 获取相对于项目根目录的绝对路径
    /// </summary>
    /// <param name="relativePath">相对路径</param>
    /// <returns>绝对路径</returns>
    string GetAbsolutePath(string relativePath);
    
    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    /// <param name="fileName">配置文件名</param>
    /// <returns>配置文件完整路径</returns>
    string GetConfigFilePath(string fileName);
    
    /// <summary>
    /// 获取日志文件路径
    /// </summary>
    /// <param name="fileName">日志文件名</param>
    /// <returns>日志文件完整路径</returns>
    string GetLogFilePath(string fileName);
    
    /// <summary>
    /// 获取模板文件路径
    /// </summary>
    /// <param name="fileName">模板文件名</param>
    /// <returns>模板文件完整路径</returns>
    string GetTemplateFilePath(string fileName);
    
    /// <summary>
    /// 获取资源文件路径
    /// </summary>
    /// <param name="relativePath">相对于资源目录的路径</param>
    /// <returns>资源文件完整路径</returns>
    string GetAssetFilePath(string relativePath);
    
    /// <summary>
    /// 验证路径是否在项目范围内
    /// </summary>
    /// <param name="path">要验证的路径</param>
    /// <returns>是否在项目范围内</returns>
    bool IsPathInProject(string path);
    
    /// <summary>
    /// 清理临时文件
    /// </summary>
    void CleanupTempFiles();
}