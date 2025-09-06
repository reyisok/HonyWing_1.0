using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HonyWing.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace HonyWing.Infrastructure.Services;

/// <summary>
/// 全局热键服务实现
/// @author: Mr.Rey Copyright © 2025
/// @created: 2025-01-17 11:05:00
/// @version: 1.0.0
/// </summary>
public class GlobalHotkeyService : IGlobalHotkeyService
{
    private readonly ILogger<GlobalHotkeyService> _logger;
    private readonly Dictionary<int, Action> _hotkeyCallbacks;
    private readonly Dictionary<int, (HotkeyModifiers modifiers, HotkeyKeys key)> _registeredHotkeys;
    private readonly HotkeyMessageWindow _messageWindow;
    private bool _isEnabled;
    private bool _disposed;

    public GlobalHotkeyService(ILogger<GlobalHotkeyService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hotkeyCallbacks = new Dictionary<int, Action>();
        _registeredHotkeys = new Dictionary<int, (HotkeyModifiers, HotkeyKeys)>();
        _messageWindow = new HotkeyMessageWindow(this);
        _isEnabled = true;
        
        _logger.LogDebug("全局热键服务已初始化");
    }

    public bool IsEnabled => _isEnabled && !_disposed;

    public bool RegisterHotkey(int id, HotkeyModifiers modifiers, HotkeyKeys key, Action callback)
    {
        if (_disposed)
        {
            _logger.LogWarning("服务已释放，无法注册热键 ID: {Id}", id);
            return false;
        }

        if (_registeredHotkeys.ContainsKey(id))
        {
            _logger.LogWarning("热键 ID {Id} 已存在，请先注销后再注册", id);
            return false;
        }

        try
        {
            bool success = RegisterHotKey(_messageWindow.Handle, id, (uint)modifiers, (uint)key);
            if (success)
            {
                _hotkeyCallbacks[id] = callback;
                _registeredHotkeys[id] = (modifiers, key);
                _logger.LogDebug("成功注册全局热键 ID: {Id}, 修饰键: {Modifiers}, 按键: {Key}", id, modifiers, key);
                return true;
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                _logger.LogError("注册全局热键失败 ID: {Id}, 错误代码: {Error}", id, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注册全局热键时发生异常 ID: {Id}", id);
            return false;
        }
    }

    public bool UnregisterHotkey(int id)
    {
        if (_disposed)
        {
            _logger.LogWarning("服务已释放，无法注销热键 ID: {Id}", id);
            return false;
        }

        if (!_registeredHotkeys.ContainsKey(id))
        {
            _logger.LogWarning("热键 ID {Id} 未注册，无需注销", id);
            return true;
        }

        try
        {
            bool success = UnregisterHotKey(_messageWindow.Handle, id);
            if (success)
            {
                _hotkeyCallbacks.Remove(id);
                _registeredHotkeys.Remove(id);
                _logger.LogDebug("成功注销全局热键 ID: {Id}", id);
                return true;
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                _logger.LogError("注销全局热键失败 ID: {Id}, 错误代码: {Error}", id, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注销全局热键时发生异常 ID: {Id}", id);
            return false;
        }
    }

    public void UnregisterAllHotkeys()
    {
        if (_disposed)
        {
            _logger.LogWarning("服务已释放，无法注销所有热键");
            return;
        }

        var hotkeyIds = new List<int>(_registeredHotkeys.Keys);
        foreach (int id in hotkeyIds)
        {
            UnregisterHotkey(id);
        }
        
        _logger.LogDebug("已注销所有全局热键");
    }

    public bool IsHotkeyRegistered(int id)
    {
        return !_disposed && _registeredHotkeys.ContainsKey(id);
    }

    public void Enable()
    {
        if (_disposed)
        {
            _logger.LogWarning("服务已释放，无法启用");
            return;
        }

        _isEnabled = true;
        _logger.LogDebug("全局热键服务已启用");
    }

    public void Disable()
    {
        if (_disposed)
        {
            _logger.LogWarning("服务已释放，无法禁用");
            return;
        }

        _isEnabled = false;
        _logger.LogDebug("全局热键服务已禁用");
    }

    internal void OnHotkeyPressed(int id)
    {
        if (!_isEnabled || _disposed)
        {
            return;
        }

        if (_hotkeyCallbacks.TryGetValue(id, out Action? callback))
        {
            try
            {
                _logger.LogDebug("触发全局热键 ID: {Id}", id);
                callback?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行热键回调时发生异常 ID: {Id}", id);
            }
        }
        else
        {
            _logger.LogWarning("未找到热键回调 ID: {Id}", id);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            UnregisterAllHotkeys();
            _messageWindow?.Dispose();
            _disposed = true;
            _logger.LogDebug("全局热键服务已释放");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "释放全局热键服务时发生异常");
        }
    }

    #region Windows API

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int WM_HOTKEY = 0x0312;

    #endregion

    #region Message Window

    private class HotkeyMessageWindow : NativeWindow, IDisposable
    {
        private readonly GlobalHotkeyService _service;
        private bool _disposed;

        public HotkeyMessageWindow(GlobalHotkeyService service)
        {
            _service = service;
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && !_disposed)
            {
                int id = m.WParam.ToInt32();
                _service.OnHotkeyPressed(id);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (Handle != IntPtr.Zero)
                {
                    DestroyHandle();
                }
            }
        }
    }

    #endregion
}