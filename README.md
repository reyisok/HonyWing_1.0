# HonyWing - Intelligent Mouse Automation Tool

<div align="center">

![License](https://img.shields.io/badge/license-MIT%20Modified-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows%2011-blue.svg)
![Language](https://img.shields.io/badge/language-C%23-green.svg)

An intelligent mouse automation tool based on image recognition, optimized for Windows 11 systems.

</div>

## ✨ Features

### 🎯 Core Functions

- **Intelligent Image Matching**: High-precision template matching algorithm based on OpenCV
- **Screen Area Monitoring**: Support for full screen, window, and custom area monitoring
- **Mouse Simulation Operations**: Precise mouse clicking, moving, and dragging simulation
- **Multi-target Detection**: Simultaneous monitoring of multiple target images with intelligent sorting and matching
- **Real-time Status Monitoring**: Detailed runtime logs and matching records

### 🔧 Advanced Features

- **DPI Adaptive**: Perfect support for high-resolution displays and multi-monitor environments
- **Click Animation Effects**: Visual click position feedback with intuitive animations
- **Configuration Management**: Support for saving, loading, importing, and exporting configuration files
- **Hotkey Operations**: Rich hotkey support for enhanced operational efficiency
- **System Tray Operation**: Support for minimizing to system tray with silent background operation

### 🎨 User Experience

- **Modern Interface**: Adopts ModernWpf design, conforming to Windows 11 design language
- **Deep Blue Theme**: Unified visual style that's comfortable and eye-friendly
- **Responsive Layout**: Adapts to different screen sizes and DPI settings
- **Intuitive Operations**: Drag-and-drop upload, area selection, one-click start/stop

## 🚀 Quick Start

### System Requirements

- **Operating System**: Windows 11 (recommended) or Windows 10 1903+
- **Runtime**: .NET 8.0 Runtime
- **Memory**: At least 512MB available memory
- **Display**: Support for any resolution and DPI settings

### Installation Steps

1. **Download Release Version**

   ```bash
   # Download the latest version from the Releases page
   # Or clone the source code and compile yourself
   git clone https://github.com/reyisok/HonyWing_1.0.git
   ```

2. **Compile and Run** (Developers)

   ```bash
   cd HonyWing
   dotnet restore
   dotnet build
   dotnet run --project src\HonyWing.UI\HonyWing.UI.csproj
   ```

3. **First Use**
   - Launch the application
   - Upload target images (supports PNG, JPG, BMP formats)
   - Set monitoring area
   - Configure matching parameters
   - Click the "Start Matching" button

## 📖 User Guide

### Basic Operation Flow

1. **Image Management**
   - Click the "Select Image" button or drag images to the upload area
   - View and adjust target images in the preview area
   - Support multi-image management with add, delete, and sort functions

2. **Area Settings**
   - Select monitoring mode: full screen, current window, custom area
   - Use the "Select Area" tool to precisely frame the monitoring range
   - Real-time preview of selected area

3. **Parameter Configuration**
   - **Match Precision**: Adjust similarity threshold (0.1-1.0)
   - **Click Delay**: Set click interval time
   - **Click Type**: Left click, right click, double click
   - **Advanced Options**: Smooth movement, click animation, etc.

4. **Runtime Control**
   - **Start Matching**: Begin automatic monitoring and clicking
   - **Pause/Resume**: Temporarily pause or resume operation
   - **Stop**: Completely stop monitoring
   - **Hotkeys**: Space (pause/resume), Esc (stop)

### Advanced Features

#### DPI Adaptation

- Automatic detection of system DPI settings
- Support for common scaling ratios like 100%, 125%, 150%, 200%
- Intelligent coordinate conversion in multi-monitor environments

#### Configuration Management

```json
{
  "MatchThreshold": 0.8,
  "ClickDelay": 1000,
  "ClickType": "LeftClick",
  "MonitoringArea": {
    "Type": "CustomArea",
    "X": 100,
    "Y": 100,
    "Width": 800,
    "Height": 600
  }
}
```

## 🏗️ Technical Architecture

### Project Structure

```
HonyWing/
├── src/
│   ├── HonyWing.Core/           # Core business logic
│   │   ├── Interfaces/          # Interface definitions
│   │   ├── Models/              # Data models
│   │   └── Services/            # Business services
│   ├── HonyWing.Infrastructure/ # Infrastructure layer
│   │   └── Services/            # Infrastructure service implementations
│   └── HonyWing.UI/            # WPF user interface
│       ├── Views/              # Views
│       ├── ViewModels/         # View models
│       ├── Controls/           # Custom controls
│       ├── Converters/         # Value converters
│       └── Styles/             # Style resources
├── docs/                       # Project documentation
├── tools/                      # Development tools
└── test/                       # Test projects
```

### Technology Stack

- **Framework**: .NET 8.0 + WPF
- **Language**: C# 12.0
- **Image Processing**: OpenCVSharp4
- **UI Library**: ModernWpf
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Logging System**: NLog
- **Configuration Management**: System.Text.Json

### Core Components

#### Image Matching Engine

- Based on OpenCV template matching algorithm
- Support for multi-scale matching and rotation invariance
- Optimized matching performance, < 200ms on 4K screens

#### DPI Adaptation System

- Real-time DPI detection and coordinate conversion
- Support for dynamic DPI changes
- Multi-monitor environment adaptation

#### Mouse Simulation Service

- Based on Windows API (user32.dll)
- Support for natural mouse movement trajectories
- Configurable click delays and animation effects

## 🤝 Contributing Guidelines

We welcome community contributions! Please follow these steps:

1. **Fork the Project**
2. **Create a Feature Branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit Changes** (`git commit -m 'Add some AmazingFeature'`)
4. **Push to Branch** (`git push origin feature/AmazingFeature`)
5. **Create Pull Request**

### Development Standards

- Follow C# coding conventions
- Add appropriate unit tests
- Update relevant documentation
- Ensure code passes all tests

## 📄 Open Source License

This project is licensed under the Modified MIT License. See the [LICENSE.txt](LICENSE.txt) file for details.

### License Key Points

- ✅ Personal learning and research use
- ✅ Non-commercial personal projects
- ❌ Commercial use requires written permission
- ❌ Enterprise/organizational internal use requires authorization

## 🙏 Acknowledgments

Thanks to the following open source projects for their support:

- [OpenCVSharp](https://github.com/shimat/opencvsharp) - Image processing library
- [ModernWpf](https://github.com/Kinnara/ModernWpf) - Modern WPF UI library
- [NLog](https://github.com/NLog/NLog) - Logging framework
- [Microsoft.Toolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM toolkit

## 📞 Contact Information

- **Author**: Mr.Rey
- **Email**: [reyisok@live.com]
- **Project Homepage**: [https://github.com/reyisok/HonyWing_1.0]

## Acknowledgment to Pioneers

Sincere gratitude to all the pioneers who have blazed trails in the field of technology. As a beginner, it is by standing on your shoulders and drawing nourishment from the experiences and explorations of predecessors that I have been able to break through the limitations of thinking and implement ideas into various functional modules in HonyWing for learning and practice, giving substance to the joy of technological exploration.

Special thanks to every user of HonyWing. As an experimental system limited to personal learning purposes, its core value comes from your experiences and feedback — whether it's usage suggestions for screen monitoring, color recognition, text recognition, or image matching functions, or problems discovered during learning simulation operations and testing recognition processes, these real feedbacks have become important directions for system optimization and have helped me, as a beginner, more clearly understand the core goal of "technology serving learning."

It should be noted that this project is still in the improvement stage, with function polishing and experience optimization continuing to advance. As a beginner, I inevitably have areas of insufficient consideration during development. If you encounter operational anomalies, recognition deviations, or other issues during use, please be tolerant and feel free to provide improvement suggestions at any time. Your tolerance and suggestions will become important assistance for my growth and project improvement.

Throughout the learning and development process of the entire project, I have gained a deeper understanding of the weight of the word "standards." A clear set of code standards and mature development best practices not only allow me, as a beginner, to more easily organize code logic, but also clear obstacles for subsequent project maintenance and feature expansion. Those practice-verified standard guidelines, like lighthouses on the technical path, have helped me avoid many detours and kept this experimental project always maintaining the vitality of being iterable and optimizable.

Finally, may every explorer find joy in technical learning. Enjoy your lucky day!

Mr. Rey
By HonyWing Team (including AI Assistant)

---

<div align="center">

**If this project helps you, please consider giving it a ⭐**

</div>
