using System;

namespace HonyWing.Core.Interfaces;

/// <summary>
/// 通知服务接口
/// @author: Mr.Rey Copyright © 2025
/// @created: 2025-01-27 15:30:00
/// @version: 1.0.0
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// 显示信息通知
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    void ShowInfo(string message, string title = "信息");

    /// <summary>
    /// 显示警告通知
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    void ShowWarning(string message, string title = "警告");

    /// <summary>
    /// 显示错误通知
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    void ShowError(string message, string title = "错误");

    /// <summary>
    /// 显示确认对话框
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    /// <returns>用户是否确认</returns>
    bool ShowConfirm(string message, string title = "确认");

    /// <summary>
    /// 显示成功通知
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题</param>
    void ShowSuccess(string message, string title = "成功");

    /// <summary>
    /// 显示输入对话框
    /// </summary>
    /// <param name="message">提示消息</param>
    /// <param name="title">标题</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>用户输入的文本，如果取消则返回null</returns>
    string? ShowInput(string message, string title = "输入", string defaultValue = "");
}