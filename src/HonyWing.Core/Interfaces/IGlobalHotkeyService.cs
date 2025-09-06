using System;

namespace HonyWing.Core.Interfaces;

/// <summary>
/// 全局热键服务接口
/// @author: Mr.Rey Copyright © 2025
/// @created: 2025-01-17 11:00:00
/// @version: 1.0.0
/// </summary>
public interface IGlobalHotkeyService : IDisposable
{
    /// <summary>
    /// 注册全局热键
    /// </summary>
    /// <param name="id">热键ID</param>
    /// <param name="modifiers">修饰键</param>
    /// <param name="key">按键</param>
    /// <param name="callback">回调函数</param>
    /// <returns>是否注册成功</returns>
    bool RegisterHotkey(int id, HotkeyModifiers modifiers, HotkeyKeys key, Action callback);

    /// <summary>
    /// 注销全局热键
    /// </summary>
    /// <param name="id">热键ID</param>
    /// <returns>是否注销成功</returns>
    bool UnregisterHotkey(int id);

    /// <summary>
    /// 注销所有热键
    /// </summary>
    void UnregisterAllHotkeys();

    /// <summary>
    /// 检查热键是否已注册
    /// </summary>
    /// <param name="id">热键ID</param>
    /// <returns>是否已注册</returns>
    bool IsHotkeyRegistered(int id);

    /// <summary>
    /// 启用热键处理
    /// </summary>
    void Enable();

    /// <summary>
    /// 禁用热键处理
    /// </summary>
    void Disable();

    /// <summary>
    /// 获取是否启用状态
    /// </summary>
    bool IsEnabled { get; }
}

/// <summary>
/// 热键修饰键枚举
/// </summary>
[Flags]
public enum HotkeyModifiers : uint
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Win = 8
}

/// <summary>
/// 热键按键枚举
/// </summary>
public enum HotkeyKeys : uint
{
    None = 0,
    Space = 0x20,
    Escape = 0x1B,
    F1 = 0x70,
    F2 = 0x71,
    F3 = 0x72,
    F4 = 0x73,
    F5 = 0x74,
    F6 = 0x75,
    F7 = 0x76,
    F8 = 0x77,
    F9 = 0x78,
    F10 = 0x79,
    F11 = 0x7A,
    F12 = 0x7B
}