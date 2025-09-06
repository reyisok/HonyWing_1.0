using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace HonyWing.UI.Views;

/// <summary>
/// 输入对话框
/// @author: Mr.Rey Copyright © 2025
/// @created: 2025-01-27 16:50:00
/// @version: 1.0.0
/// </summary>
public partial class InputDialog : Window, INotifyPropertyChanged
{
    private string _message = string.Empty;
    private string _inputText = string.Empty;

    public InputDialog(string message, string title = "输入", string defaultValue = "")
    {
        InitializeComponent();
        DataContext = this;
        
        Title = title;
        Message = message;
        InputText = defaultValue;
        
        // 设置焦点到输入框
        Loaded += (s, e) =>
        {
            InputTextBox.Focus();
            InputTextBox.SelectAll();
        };
    }

    /// <summary>
    /// 提示消息
    /// </summary>
    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 输入文本
    /// </summary>
    public string InputText
    {
        get => _inputText;
        set
        {
            _inputText = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 确定按钮点击事件
    /// </summary>
    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}