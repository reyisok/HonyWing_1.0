using HonyWing.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace HonyWing.Infrastructure.Services;

/// <summary>
/// 通知服务实现 - 将所有通知记录到日志而不显示弹窗
/// @author: Mr.Rey Copyright © 2025
/// @created: 2025-01-27 15:32:00
/// @version: 1.0.0
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 显示信息通知
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    public void ShowInfo(string message, string title = "信息")
    {
        _logger.LogInformation("[{Title}] {Message}", title, message);
    }

    /// <summary>
    /// 显示警告通知
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    public void ShowWarning(string message, string title = "警告")
    {
        _logger.LogWarning("[{Title}] {Message}", title, message);
    }

    /// <summary>
    /// 显示错误通知
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    public void ShowError(string message, string title = "错误")
    {
        _logger.LogError("[{Title}] {Message}", title, message);
    }

    /// <summary>
    /// 显示确认对话框
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    /// <returns>默认返回true（自动确认）</returns>
    public bool ShowConfirm(string message, string title = "确认")
    {
        _logger.LogInformation("[{Title}] 确认对话框: {Message} - 自动确认", title, message);
        return true; // 默认自动确认以避免阻塞
    }

    /// <summary>
    /// 显示成功通知
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    public void ShowSuccess(string message, string title = "成功")
    {
        _logger.LogInformation("[{Title}] ✓ {Message}", title, message);
    }

    /// <summary>
    /// 显示输入对话框
    /// </summary>
    /// <param name="message">提示消息</param>
    /// <param name="title">标题</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>用户输入的文本，如果取消则返回默认值</returns>
    public string? ShowInput(string message, string title = "输入", string defaultValue = "")
    {
        _logger.LogInformation("[{Title}] 输入对话框: {Message} - 返回默认值: {DefaultValue}", title, message, defaultValue);
        return defaultValue; // 返回默认值以避免阻塞
    }
}