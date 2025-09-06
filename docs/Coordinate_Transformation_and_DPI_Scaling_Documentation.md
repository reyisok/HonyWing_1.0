# Coordinate Transformation Service and DPI Scaling Processing Documentation

**@author: Mr.Rey Copyright © 2025**  
**@created: 2025-01-07**  
**@version: 1.0.0**

## 1. Overview

This document provides detailed information about the coordinate transformation service and DPI scaling processing implementation in the HonyWing project, including service architecture, core algorithms, usage methods, and best practices.

### 1.1 Core Objectives

- **Precise Coordinate Transformation**: Ensure all screen interactions (screenshots, mouse clicks, region selection) are based on actual pixel calculations
- **Automatic DPI Adaptation**: Automatically complete Device Independent Pixel (DIP) → Actual Pixel conversion
- **Multi-Monitor Support**: Handle DPI differences between different monitors
- **Real-time Response**: Detect DPI setting changes and automatically adjust

### 1.2 Technical Requirements

- DPI Detection Accuracy: ≥99% (If system setting is 125%, detection result should be 125%)
- Coordinate Transformation Error: ≤1 pixel
- Multi-monitor Switch Response Time: ≤1 second
- CPU Usage: ≤5% (normal operation state)

## 2. Service Architecture

### 2.1 Core Service Components

```
Coordinate Transformation & DPI Processing Architecture
├── IDpiAdaptationService (Interface)
│   └── DpiAdaptationService (Implementation)
├── IScreenCaptureService (Interface)
│   └── ScreenCaptureService (Implementation)
├── IMouseService (Interface)
│   └── MouseService (Implementation)
└── Related Support Services
    ├── ImageMatcherService
    ├── ClickAnimationService
    └── RegionSelectionWindow
```

### 2.2 Service Dependencies

- **DpiAdaptationService**: Core DPI processing service, depended upon by other services
- **ScreenCaptureService**: Depends on DPI service for coordinate transformation
- **MouseService**: Uses physical coordinates for mouse operations
- **ImageMatcherService**: Depends on DPI service to adjust search regions
- **ClickAnimationService**: Depends on DPI service to convert display coordinates

## 3. DPI Adaptation Service Details

### 3.1 Interface Definition

**File Location**: `src/HonyWing.Core/Interfaces/IDpiAdaptationService.cs`

#### Core Methods

```csharp
public interface IDpiAdaptationService
{
    // DPI Information Retrieval
    double GetSystemDpiScale();
    double GetMonitorDpiScale(IntPtr monitorHandle);
    double GetDpiScaleForPoint(System.Drawing.Point point);
    double GetDpiScaleForWindow(IntPtr windowHandle);
    
    // Coordinate Transformation
    System.Drawing.Point DipToPixel(System.Drawing.Point dipPoint);
    System.Drawing.Point PixelToDip(System.Drawing.Point pixelPoint);
    Rectangle DipToPixel(Rectangle dipRect);
    Rectangle PixelToDip(Rectangle pixelRect);
    
    // DPI Change Monitoring
    bool HasDpiChanged();
    void RefreshDpiInfo();
    string GetDpiInfoString();
    
    // Events
    event EventHandler<DpiChangedEventArgs>? DpiChanged;
}
```

### 3.2 Implementation Details

**File Location**: `src/HonyWing.Infrastructure/Services/DpiAdaptationService.cs`

#### 3.2.1 Windows API Calls

```csharp
// Core API Functions
[DllImport("user32.dll")]
private static extern IntPtr GetDC(IntPtr hWnd);

[DllImport("gdi32.dll")]
private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

[DllImport("shcore.dll")]
private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

[DllImport("user32.dll")]
private static extern IntPtr MonitorFromPoint(System.Drawing.Point pt, uint dwFlags);

[DllImport("user32.dll")]
private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
```

#### 3.2.2 DPI Scale Ratio Calculation

```csharp
public double GetSystemDpiScale()
{
    try
    {
        IntPtr hdc = GetDC(IntPtr.Zero);
        int dpiX = GetDeviceCaps(hdc, LOGPIXELSX); // 88
        ReleaseDC(IntPtr.Zero, hdc);
        
        double scale = dpiX / 96.0; // 96 DPI is standard
        return scale;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to get system DPI scale ratio");
        return 1.0; // Default 100% scaling
    }
}
```

#### 3.2.3 Coordinate Transformation Algorithm

**DIP to Pixel Conversion**:
```csharp
public System.Drawing.Point DipToPixel(System.Drawing.Point dipPoint)
{
    lock (_lockObject)
    {
        int pixelX = (int)Math.Round(dipPoint.X * _currentDpiScale);
        int pixelY = (int)Math.Round(dipPoint.Y * _currentDpiScale);
        return new System.Drawing.Point(pixelX, pixelY);
    }
}
```

**Pixel to DIP Conversion**:
```csharp
public System.Drawing.Point PixelToDip(System.Drawing.Point pixelPoint)
{
    lock (_lockObject)
    {
        int dipX = (int)Math.Round(pixelPoint.X / _currentDpiScale);
        int dipY = (int)Math.Round(pixelPoint.Y / _currentDpiScale);
        return new System.Drawing.Point(dipX, dipY);
    }
}
```

## 4. Screen Capture Service Coordinate Processing

### 4.1 Service Overview

**File Location**: `src/HonyWing.Core/Services/ScreenCaptureService.cs`

The screen capture service is responsible for capturing screen content and requires precise coordinate transformation to ensure capture accuracy.

### 4.2 Coordinate Transformation Methods

#### 4.2.1 Full Screen Capture

```csharp
public Bitmap CaptureFullScreen()
{
    try
    {
        // Get actual screen size (physical pixels)
        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        int screenHeight = GetSystemMetrics(SM_CYSCREEN);
        
        // Create bitmap using physical pixel size
        Bitmap bitmap = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppArgb);
        
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            // Capture using physical coordinates
            graphics.CopyFromScreen(0, 0, 0, 0, new Size(screenWidth, screenHeight));
        }
        
        return bitmap;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Full screen capture failed");
        throw;
    }
}
```

#### 4.2.2 Region Capture

```csharp
public Bitmap CaptureRegion(Rectangle region)
{
    try
    {
        // Convert DIP coordinates to physical pixel coordinates
        Rectangle physicalRegion = _dpiAdaptationService.DipToPixel(region);
        
        // Boundary check
        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        int screenHeight = GetSystemMetrics(SM_CYSCREEN);
        
        physicalRegion = Rectangle.Intersect(physicalRegion, 
            new Rectangle(0, 0, screenWidth, screenHeight));
        
        if (physicalRegion.IsEmpty)
        {
            throw new ArgumentException("Capture region is outside screen bounds");
        }
        
        // Create bitmap and capture
        Bitmap bitmap = new Bitmap(physicalRegion.Width, physicalRegion.Height, 
            PixelFormat.Format32bppArgb);
        
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(physicalRegion.Location, Point.Empty, 
                physicalRegion.Size);
        }
        
        return bitmap;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Region capture failed: {Region}", region);
        throw;
    }
}
```

## 5. Mouse Service Coordinate Processing

### 5.1 Service Overview

**File Location**: `src/HonyWing.Core/Services/MouseService.cs`

The mouse service handles mouse simulation operations and must use physical pixel coordinates to ensure click accuracy.

### 5.2 Coordinate Transformation Methods

#### 5.2.1 Mouse Click

```csharp
public void Click(Point position)
{
    try
    {
        // Convert DIP coordinates to physical pixel coordinates
        Point physicalPosition = _dpiAdaptationService.DipToPixel(position);
        
        // Boundary check
        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        int screenHeight = GetSystemMetrics(SM_CYSCREEN);
        
        if (physicalPosition.X < 0 || physicalPosition.X >= screenWidth ||
            physicalPosition.Y < 0 || physicalPosition.Y >= screenHeight)
        {
            _logger.LogWarning("Click position outside screen bounds: {Position}", 
                physicalPosition);
            return;
        }
        
        // Set cursor position (using physical coordinates)
        SetCursorPos(physicalPosition.X, physicalPosition.Y);
        
        // Simulate mouse click
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        Thread.Sleep(50); // Click duration
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        
        _logger.LogDebug("Mouse click executed at physical position: {Position}", 
            physicalPosition);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Mouse click failed at position: {Position}", position);
        throw;
    }
}
```

#### 5.2.2 Get Current Mouse Position

```csharp
public Point GetCurrentPosition()
{
    try
    {
        GetCursorPos(out Point physicalPosition);
        
        // Convert physical coordinates to DIP coordinates
        Point dipPosition = _dpiAdaptationService.PixelToDip(physicalPosition);
        
        return dipPosition;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to get current mouse position");
        return Point.Empty;
    }
}
```

## 6. Image Matching Service Coordinate Processing

### 6.1 Service Overview

**File Location**: `src/HonyWing.Core/Services/ImageMatcherService.cs`

The image matching service needs to handle coordinate transformation during template matching to ensure search accuracy.

### 6.2 Coordinate Transformation in Search

```csharp
public Point? FindImageInRegion(Bitmap templateImage, Rectangle searchRegion, double threshold = 0.8)
{
    try
    {
        // Convert search region to physical pixel coordinates
        Rectangle physicalSearchRegion = _dpiAdaptationService.DipToPixel(searchRegion);
        
        // Capture search region
        using (Bitmap searchBitmap = _screenCaptureService.CaptureRegion(physicalSearchRegion))
        {
            // Perform template matching
            var matchResult = PerformTemplateMatching(templateImage, searchBitmap, threshold);
            
            if (matchResult.HasValue)
            {
                // Convert match result to global physical coordinates
                Point globalPhysicalPoint = new Point(
                    physicalSearchRegion.X + matchResult.Value.X,
                    physicalSearchRegion.Y + matchResult.Value.Y
                );
                
                // Convert to DIP coordinates for return
                Point dipPoint = _dpiAdaptationService.PixelToDip(globalPhysicalPoint);
                
                _logger.LogDebug("Image found at DIP position: {Position}", dipPoint);
                return dipPoint;
            }
        }
        
        return null;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Image matching failed in region: {Region}", searchRegion);
        return null;
    }
}
```

## 7. Click Animation Service Coordinate Processing

### 7.1 Service Overview

**File Location**: `src/HonyWing.UI/Services/ClickAnimationService.cs`

The click animation service displays visual feedback at click positions and requires coordinate transformation from physical coordinates to WPF logical coordinates.

### 7.2 Coordinate Transformation Methods

```csharp
public void ShowClickAnimation(Point clickPosition)
{
    try
    {
        // Convert physical pixel coordinates to WPF logical coordinates
        double dpiScale = _dpiAdaptationService.GetSystemDpiScale();
        double logicalX = clickPosition.X / dpiScale;
        double logicalY = clickPosition.Y / dpiScale;
        
        // Create and display animation window
        Application.Current.Dispatcher.Invoke(() =>
        {
            var animationWindow = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                Topmost = true,
                ShowInTaskbar = false,
                Width = 50,
                Height = 50,
                Left = logicalX - 25, // Center display
                Top = logicalY - 25
            };
            
            // Display animation...
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to show click animation");
    }
}
```

## 8. Region Selection Window Coordinate Processing

### 8.1 Service Overview

**File Location**: `src/HonyWing.UI/Views/RegionSelectionWindow.xaml.cs`

The region selection window needs to handle conversion between WPF logical coordinates and screen physical coordinates.

### 8.2 Coordinate Transformation Methods

```csharp
/// <summary>
/// Get selection region relative to screen (physical pixel coordinates)
/// </summary>
public Rect GetScreenRegion()
{
    if (!HasSelection)
        return Rect.Empty;
    
    // Convert to screen coordinates (WPF logical coordinates to physical coordinates)
    var screenRect = new Rect(
        _selectedRegion.Left + Left,
        _selectedRegion.Top + Top,
        _selectedRegion.Width,
        _selectedRegion.Height
    );
    
    return screenRect;
}
```

## 9. DPI Change Handling Mechanism

### 9.1 DPI Change Detection

```csharp
public bool HasDpiChanged()
{
    double newDpiScale = GetSystemDpiScale();
    lock (_lockObject)
    {
        return Math.Abs(newDpiScale - _currentDpiScale) > 0.01; // Allow 1% error
    }
}
```

### 9.2 DPI Change Event Handling

**File Location**: `src/HonyWing.UI/ViewModels/MainWindowViewModel.cs`

```csharp
private void OnDpiChanged(object? sender, DpiChangedEventArgs e)
{
    try
    {
        // Update DPI information
        SystemDpiX = 96 * e.NewDpiScale;
        SystemDpiY = 96 * e.NewDpiScale;
        DpiScaleX = e.NewDpiScale;
        DpiScaleY = e.NewDpiScale;
        
        DpiInfo = _dpiAdaptationService.GetDpiInfoString();
        
        _logger.LogInformation("DPI change processing completed - Old scale: {OldScale}%, New scale: {NewScale}%",
            e.OldDpiScale * 100, e.NewDpiScale * 100);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to handle DPI change event");
    }
}
```

## 10. Best Practices and Considerations

### 10.1 Coordinate System Standards

1. **Physical Pixel Coordinates**: Used for all Windows API calls (screenshots, mouse operations)
2. **WPF Logical Coordinates**: Used for WPF interface element positioning and animation
3. **DIP Coordinates**: Used for cross-DPI coordinate calculation and storage

### 10.2 Transformation Timing

- **Input Phase**: User interface coordinates → DIP coordinates
- **Processing Phase**: DIP coordinates → Physical pixel coordinates
- **Output Phase**: Physical pixel coordinates → Interface display coordinates

### 10.3 Performance Optimization

1. **Cache DPI Information**: Avoid frequent Windows API calls
2. **Batch Conversion**: Perform batch conversion for multiple coordinate points
3. **Thread Safety**: Use locking mechanism to protect DPI cache

### 10.4 Error Handling

1. **API Call Failure**: Provide default values (1.0 scale ratio)
2. **Coordinate Out of Bounds**: Perform boundary checking and correction
3. **Precision Loss**: Use Math.Round for rounding

### 10.5 Testing and Validation

1. **Multi-DPI Environment Testing**: 100%, 125%, 150%, 200% scaling
2. **Multi-Monitor Testing**: Different DPI monitor combinations
3. **Dynamic DPI Change Testing**: Modify system DPI settings at runtime
4. **Precision Validation**: Coordinate transformation error ≤1 pixel

## 11. Troubleshooting

### 11.1 Common Issues

1. **Click Position Offset**
   - Cause: DIP coordinates not properly converted to physical pixel coordinates
   - Solution: Check coordinate transformation call chain

2. **Incorrect Screenshot Region**
   - Cause: Inconsistent coordinate systems for search region
   - Solution: Ensure using physical pixel coordinates for screenshots

3. **Incorrect Animation Position**
   - Cause: Physical coordinates not converted to WPF logical coordinates
   - Solution: Use DPI scale ratio for conversion

### 11.2 Debugging Methods

1. **Enable Detailed Logging**: Set log level to Debug
2. **Coordinate Comparison**: Record coordinate values before and after transformation
3. **DPI Information Monitoring**: Real-time display of current DPI status

## 12. Version Update History

| Version | Date | Update Content |
|---------|------|----------------|
| 1.0.0 | 2025-01-07 | Initial version, complete coordinate transformation and DPI processing documentation |

---

**Document Maintenance**: This document should be maintained synchronously with code updates to ensure accuracy and timeliness.
**Technical Support**: For questions, please refer to source code comments or contact the development team.