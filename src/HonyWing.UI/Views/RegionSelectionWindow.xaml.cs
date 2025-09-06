using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace HonyWing.UI.Views;

/// <summary>
/// 区域选择窗口
/// </summary>
public partial class RegionSelectionWindow : Window
{
    private bool _isSelecting;
    private bool _isResizing;
    private Point _startPoint;
    private Point _currentPoint;
    private Rectangle? _resizingHandle;
    private Rect _selectedRegion;

    public RegionSelectionWindow()
    {
        InitializeComponent();
        
        // 设置窗口覆盖所有屏幕
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
        
        // 初始化
        _selectedRegion = Rect.Empty;
        
        // 显示十字准线
        ShowCrosshair(true);
    }

    #region Properties

    /// <summary>
    /// 选择的区域
    /// </summary>
    public Rect SelectedRegion => _selectedRegion;

    /// <summary>
    /// 是否已选择区域
    /// </summary>
    public bool HasSelection => !_selectedRegion.IsEmpty && _selectedRegion.Width > 0 && _selectedRegion.Height > 0;

    #endregion

    #region Mouse Events

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_isResizing) return;
        
        _startPoint = e.GetPosition(this);
        _currentPoint = _startPoint;
        _isSelecting = true;
        
        // 隐藏帮助信息
        HelpPanel.Visibility = Visibility.Collapsed;
        
        // 隐藏十字准线
        ShowCrosshair(false);
        
        // 开始新的选择
        StartNewSelection();
        
        CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var currentPos = e.GetPosition(this);
        
        if (_isSelecting && !_isResizing)
        {
            _currentPoint = currentPos;
            UpdateSelection();
        }
        else if (!_isSelecting && !_isResizing)
        {
            // 更新十字准线位置
            UpdateCrosshair(currentPos);
        }
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isSelecting && !_isResizing)
        {
            _isSelecting = false;
            ReleaseMouseCapture();
            
            // 完成选择
            CompleteSelection();
            
            e.Handled = true;
        }
    }

    #endregion

    #region Handle Events

    private void OnHandleMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Rectangle handle)
        {
            _resizingHandle = handle;
            _isResizing = true;
            _startPoint = e.GetPosition(this);
            
            handle.CaptureMouse();
            e.Handled = true;
        }
    }

    private void OnHandleMouseMove(object sender, MouseEventArgs e)
    {
        if (_isResizing && _resizingHandle != null && e.LeftButton == MouseButtonState.Pressed)
        {
            var currentPos = e.GetPosition(this);
            var deltaX = currentPos.X - _startPoint.X;
            var deltaY = currentPos.Y - _startPoint.Y;
            
            ResizeSelection(_resizingHandle, deltaX, deltaY);
            
            _startPoint = currentPos;
            e.Handled = true;
        }
    }

    private void OnHandleMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isResizing && _resizingHandle != null)
        {
            _resizingHandle.ReleaseMouseCapture();
            _resizingHandle = null;
            _isResizing = false;
            
            e.Handled = true;
        }
    }

    #endregion

    #region Keyboard Events

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                if (HasSelection)
                {
                    OnConfirmClick(sender, e);
                }
                break;
                
            case Key.Escape:
                OnCancelClick(sender, e);
                break;
        }
    }

    #endregion

    #region Button Events

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        if (HasSelection)
        {
            DialogResult = true;
            Close();
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    #endregion

    #region Private Methods

    private void StartNewSelection()
    {
        // 显示选择框
        SelectionRectangle.Visibility = Visibility.Visible;
        
        // 重置选择区域
        _selectedRegion = new Rect(_startPoint, _startPoint);
        
        // 更新选择框
        UpdateSelectionRectangle();
    }

    private void UpdateSelection()
    {
        // 计算选择区域
        var left = Math.Min(_startPoint.X, _currentPoint.X);
        var top = Math.Min(_startPoint.Y, _currentPoint.Y);
        var right = Math.Max(_startPoint.X, _currentPoint.X);
        var bottom = Math.Max(_startPoint.Y, _currentPoint.Y);
        
        _selectedRegion = new Rect(left, top, right - left, bottom - top);
        
        // 更新UI
        UpdateSelectionRectangle();
        UpdateInfoPanel();
    }

    private void CompleteSelection()
    {
        if (HasSelection)
        {
            // 显示调整手柄
            ShowResizeHandles(true);
            
            // 显示工具栏
            ToolbarPanel.Visibility = Visibility.Visible;
            
            // 显示信息面板
            InfoPanel.Visibility = Visibility.Visible;
            UpdateInfoPanel();
        }
        else
        {
            // 选择区域太小，重新开始
            ResetSelection();
        }
    }

    private void UpdateSelectionRectangle()
    {
        Canvas.SetLeft(SelectionRectangle, _selectedRegion.Left);
        Canvas.SetTop(SelectionRectangle, _selectedRegion.Top);
        SelectionRectangle.Width = _selectedRegion.Width;
        SelectionRectangle.Height = _selectedRegion.Height;
    }

    private void UpdateInfoPanel()
    {
        if (HasSelection)
        {
            CoordinateText.Text = $"位置: ({_selectedRegion.Left:F0}, {_selectedRegion.Top:F0})";
            SizeText.Text = $"大小: {_selectedRegion.Width:F0} × {_selectedRegion.Height:F0}";
            
            // 定位信息面板
            var panelLeft = _selectedRegion.Right + 10;
            var panelTop = _selectedRegion.Top;
            
            // 确保面板不超出屏幕边界
            if (panelLeft + 150 > Width)
            {
                panelLeft = _selectedRegion.Left - 150 - 10;
            }
            if (panelTop + 50 > Height)
            {
                panelTop = _selectedRegion.Bottom - 50;
            }
            
            Canvas.SetLeft(InfoPanel, Math.Max(10, panelLeft));
            Canvas.SetTop(InfoPanel, Math.Max(10, panelTop));
        }
    }

    private void ShowResizeHandles(bool show)
    {
        HandleCanvas.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        
        if (show && HasSelection)
        {
            UpdateResizeHandles();
        }
    }

    private void UpdateResizeHandles()
    {
        var rect = _selectedRegion;
        var handleSize = 8;
        var offset = handleSize / 2;
        
        // 角点
        Canvas.SetLeft(TopLeftHandle, rect.Left - offset);
        Canvas.SetTop(TopLeftHandle, rect.Top - offset);
        
        Canvas.SetLeft(TopRightHandle, rect.Right - offset);
        Canvas.SetTop(TopRightHandle, rect.Top - offset);
        
        Canvas.SetLeft(BottomLeftHandle, rect.Left - offset);
        Canvas.SetTop(BottomLeftHandle, rect.Bottom - offset);
        
        Canvas.SetLeft(BottomRightHandle, rect.Right - offset);
        Canvas.SetTop(BottomRightHandle, rect.Bottom - offset);
        
        // 边中点
        Canvas.SetLeft(TopHandle, rect.Left + rect.Width / 2 - offset);
        Canvas.SetTop(TopHandle, rect.Top - offset);
        
        Canvas.SetLeft(BottomHandle, rect.Left + rect.Width / 2 - offset);
        Canvas.SetTop(BottomHandle, rect.Bottom - offset);
        
        Canvas.SetLeft(LeftHandle, rect.Left - offset);
        Canvas.SetTop(LeftHandle, rect.Top + rect.Height / 2 - offset);
        
        Canvas.SetLeft(RightHandle, rect.Right - offset);
        Canvas.SetTop(RightHandle, rect.Top + rect.Height / 2 - offset);
    }

    private void ResizeSelection(Rectangle handle, double deltaX, double deltaY)
    {
        var rect = _selectedRegion;
        
        if (handle == TopLeftHandle)
        {
            rect = new Rect(rect.Left + deltaX, rect.Top + deltaY, rect.Width - deltaX, rect.Height - deltaY);
        }
        else if (handle == TopRightHandle)
        {
            rect = new Rect(rect.Left, rect.Top + deltaY, rect.Width + deltaX, rect.Height - deltaY);
        }
        else if (handle == BottomLeftHandle)
        {
            rect = new Rect(rect.Left + deltaX, rect.Top, rect.Width - deltaX, rect.Height + deltaY);
        }
        else if (handle == BottomRightHandle)
        {
            rect = new Rect(rect.Left, rect.Top, rect.Width + deltaX, rect.Height + deltaY);
        }
        else if (handle == TopHandle)
        {
            rect = new Rect(rect.Left, rect.Top + deltaY, rect.Width, rect.Height - deltaY);
        }
        else if (handle == BottomHandle)
        {
            rect = new Rect(rect.Left, rect.Top, rect.Width, rect.Height + deltaY);
        }
        else if (handle == LeftHandle)
        {
            rect = new Rect(rect.Left + deltaX, rect.Top, rect.Width - deltaX, rect.Height);
        }
        else if (handle == RightHandle)
        {
            rect = new Rect(rect.Left, rect.Top, rect.Width + deltaX, rect.Height);
        }
        
        // 确保最小尺寸
        if (rect.Width >= 10 && rect.Height >= 10)
        {
            _selectedRegion = rect;
            UpdateSelectionRectangle();
            UpdateResizeHandles();
            UpdateInfoPanel();
        }
    }

    private void ShowCrosshair(bool show)
    {
        VerticalCrosshair.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        HorizontalCrosshair.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateCrosshair(Point position)
    {
        Canvas.SetLeft(VerticalCrosshair, position.X);
        Canvas.SetTop(HorizontalCrosshair, position.Y);
    }

    private void ResetSelection()
    {
        _selectedRegion = Rect.Empty;
        SelectionRectangle.Visibility = Visibility.Collapsed;
        ShowResizeHandles(false);
        ToolbarPanel.Visibility = Visibility.Collapsed;
        InfoPanel.Visibility = Visibility.Collapsed;
        HelpPanel.Visibility = Visibility.Visible;
        ShowCrosshair(true);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 获取相对于屏幕的选择区域
    /// </summary>
    /// <returns>屏幕坐标系中的选择区域</returns>
    public Rect GetScreenRegion()
    {
        if (!HasSelection)
            return Rect.Empty;
        
        // 转换为屏幕坐标
        var screenRect = new Rect(
            _selectedRegion.Left + Left,
            _selectedRegion.Top + Top,
            _selectedRegion.Width,
            _selectedRegion.Height
        );
        
        return screenRect;
    }

    /// <summary>
    /// 设置预选区域
    /// </summary>
    /// <param name="region">要设置的区域</param>
    public void SetPreselectedRegion(Rect region)
    {
        if (region.IsEmpty || region.Width <= 0 || region.Height <= 0)
            return;
        
        // 转换为窗口坐标
        _selectedRegion = new Rect(
            region.Left - Left,
            region.Top - Top,
            region.Width,
            region.Height
        );
        
        // 更新UI
        HelpPanel.Visibility = Visibility.Collapsed;
        ShowCrosshair(false);
        
        SelectionRectangle.Visibility = Visibility.Visible;
        UpdateSelectionRectangle();
        
        CompleteSelection();
    }

    #endregion
}