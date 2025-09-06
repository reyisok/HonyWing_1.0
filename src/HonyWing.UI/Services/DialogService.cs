using System;
using System.Windows;
using System.Windows.Threading;
using HonyWing.Core.Interfaces;
using HonyWing.UI.Views;
using Microsoft.Extensions.Logging;

namespace HonyWing.UI.Services;

/// <summary>
/// WPF对话框服务实现 - 提供真实的用户交互界面
/// @author: Mr.Rey Copyright © 2025
/// @created: 2025-01-27 16:45:00
/// @version: 1.0.0
/// </summary>
public class DialogService : INotificationService
{
    private readonly ILogger<DialogService> _logger;

    public DialogService(ILogger<DialogService> logger)
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
        try
        {
            InvokeOnUIThread(() =>
            {
                // 信息消息已通过日志记录，移除MessageBox弹窗
            });
            _logger.LogInformation("显示信息: {Title} - {Message}", title, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示信息对话框失败");
        }
    }

    /// <summary>
    /// 显示警告通知
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    public void ShowWarning(string message, string title = "警告")
    {
        try
        {
            InvokeOnUIThread(() =>
            {
                // 警告消息已通过日志记录，移除MessageBox弹窗
            });
            _logger.LogWarning("[{Title}] {Message}", title, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示警告对话框失败: {Message}", message);
        }
    }

    /// <summary>
    /// 显示错误通知
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    public void ShowError(string message, string title = "错误")
    {
        try
        {
            InvokeOnUIThread(() =>
            {
                // 错误消息已通过日志记录，移除MessageBox弹窗
            });
            _logger.LogError("[{Title}] {Message}", title, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示错误对话框失败: {Message}", message);
        }
    }

    /// <summary>
    /// 显示确认对话框
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    /// <returns>用户是否确认</returns>
    public bool ShowConfirm(string message, string title = "确认")
    {
        try
        {
            bool result = false;
            InvokeOnUIThread(() =>
            {
                // 确认对话框功能暂时禁用，返回默认值
        var dialogResult = MessageBoxResult.No;
                result = dialogResult == MessageBoxResult.Yes;
            });
            
            _logger.LogInformation("显示确认对话框: {Title} - {Message}, 结果: {Result}", title, message, result ? "确认" : "取消");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示确认对话框失败");
            return false;
        }
    }

    /// <summary>
    /// 显示成功通知
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    public void ShowSuccess(string message, string title = "成功")
    {
        try
        {
            InvokeOnUIThread(() =>
            {
                // 成功消息已通过日志记录，移除MessageBox弹窗
            });
            _logger.LogInformation("[{Title}] ✓ {Message}", title, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示成功对话框失败: {Message}", message);
        }
    }

    /// <summary>
    /// 显示输入对话框
    /// </summary>
    /// <param name="message">提示消息</param>
    /// <param name="title">标题</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>用户输入的文本，如果取消则返回null</returns>
    public string? ShowInput(string message, string title = "输入", string defaultValue = "")
    {
        try
        {
            string? result = null;
            InvokeOnUIThread(() =>
            {
                var inputDialog = new InputDialog(message, title, defaultValue);
                if (inputDialog.ShowDialog() == true)
                {
                    result = inputDialog.InputText;
                }
            });
            
            _logger.LogInformation("显示输入对话框: {Title} - {Message}, 结果: {Result}", title, message, result ?? "(已取消)");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示输入对话框失败");
            return defaultValue;
        }
    }

    /// <summary>
    /// 在UI线程上执行操作
    /// </summary>
    /// <param name="action">要执行的操作</param>
    private static void InvokeOnUIThread(Action action)
    {
        if (Application.Current?.Dispatcher != null)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(action);
            }
        }
        else
        {
            // 如果没有UI线程，直接执行（用于测试场景）
            action();
        }
    }
}