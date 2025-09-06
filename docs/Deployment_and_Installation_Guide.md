# HonyWing Project Deployment and Installation Guide

**@author: Mr.Rey Copyright © 2025**
**@created: 2025-01-07**
**@version: 1.0.0**

## Project Overview

HonyWing is a WPF-based image matching and mouse automation tool that supports screen capture, image matching, mouse simulation clicks, and other features.

## System Requirements

### Minimum System Requirements

- Operating System: Windows 10 or higher
- .NET Version: .NET 8.0 or higher
- Memory: At least 4GB RAM
- Disk Space: At least 100MB available space
- Display: Support for 1920x1080 resolution or higher

### Recommended System Configuration

- Operating System: Windows 11
- .NET Version: .NET 8.0
- Memory: 8GB RAM or more
- Disk Space: 500MB available space
- Display: Support for high DPI monitors

## Development Environment Deployment

### 1. Environment Setup

#### Install .NET 8.0 SDK

1. Visit [Microsoft .NET Download Page](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Download and install .NET 8.0 SDK
3. Verify installation: Open command prompt and run `dotnet --version`

#### Install Visual Studio or Visual Studio Code

- **Visual Studio 2022** (Recommended):
  - Download Visual Studio 2022 Community or higher
  - Select ".NET desktop development" workload during installation
- **Visual Studio Code**:
  - Install C# extension
  - Install .NET Extension Pack

### 2. Project Build

#### Clone Project

```bash
git clone <project_repository_url>
cd HonyWing
```

#### Restore Dependencies

```bash
dotnet restore
```

#### Build Project

```bash
# Debug version
dotnet build --configuration Debug

# Release version
dotnet build --configuration Release
```

#### Run Project

```bash
# Run from source code
dotnet run --project src/HonyWing.UI

# Or run executable directly
cd src/HonyWing.UI/bin/Debug/net8.0-windows
./HonyWing.UI.exe
```

## Production Environment Deployment

### 1. Create Release Build

#### Self-Contained Deployment (Recommended)

```bash
# Windows x64 self-contained deployment
dotnet publish src/HonyWing.UI/HonyWing.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64

# Windows x86 self-contained deployment
dotnet publish src/HonyWing.UI/HonyWing.UI.csproj -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x86
```

#### Framework-Dependent Deployment

```bash
# Requires .NET 8.0 runtime on target machine
dotnet publish src/HonyWing.UI/HonyWing.UI.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./publish/framework-dependent
```

### 2. Deployment File Structure

File structure after publishing:

```
publish/
├── win-x64/                    # Windows x64 version
│   ├── HonyWing.UI.exe         # Main program
│   ├── appsettings.json        # Configuration file
│   ├── NLog.config            # Logging configuration
│   └── logs/                  # Log directory
├── win-x86/                    # Windows x86 version
│   └── ...
└── framework-dependent/        # Framework-dependent version
    └── ...
```

### 3. Installation and Deployment Steps

#### Method 1: Direct Deployment

1. Copy the published folder to the target machine
2. Ensure the target machine meets system requirements
3. Double-click `HonyWing.UI.exe` to run the program

#### Method 2: Create Installer Package (Optional)

Create MSI installer using WiX Toolset:

1. Install WiX Toolset v4
2. Create installer project:

```xml
<!-- HonyWing.Installer.wixproj -->
<Project Sdk="WixToolset.Sdk/4.0.0">
  <PropertyGroup>
    <OutputType>Package</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="WixToolset.UI.wixext" Version="4.0.0" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="Product.wxs" />
  </ItemGroup>
</Project>
```

3. Build installer package:

```bash
dotnet build HonyWing.Installer.wixproj -c Release
```

## Configuration

### Application Configuration (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ImageMatching": {
    "DefaultThreshold": 0.8,
    "MaxSearchTime": 5000
  },
  "MouseSimulation": {
    "ClickDelay": 100,
    "AnimationDuration": 500
  },
  "GlobalHotkeys": {
    "StopKey": "Escape"
  }
}
```

### Logging Configuration (NLog.config)

Log files are saved by default in the `logs/` directory, categorized by date and level.

## Troubleshooting

### 1. Program Won't Start

**Problem**: Double-clicking the program has no response or shows errors

**Solutions**:

- Check if .NET 8.0 runtime is installed (for framework-dependent deployment)
- Check if Windows version is supported
- Run the program as administrator
- Check if antivirus software is blocking the program

### 2. Inaccurate Image Matching

**Problem**: Image matching fails or matches wrong locations

**Solutions**:

- Adjust matching threshold (modify DefaultThreshold in appsettings.json)
- Ensure screenshots are clear and avoid blur
- Check DPI scaling settings

### 3. Global Hotkeys Not Working

**Problem**: ESC key cannot stop simulation tasks

**Solutions**:

- Check if other programs are using the ESC hotkey
- Run the program as administrator
- Restart the program to re-register hotkeys

### 4. High DPI Display Issues

**Problem**: Interface displays abnormally on high DPI monitors

**Solutions**:

- Right-click program → Properties → Compatibility → Change high DPI settings
- Select "Override high DPI scaling behavior"
- Scaling performed by: Application

## Uninstallation

### Manual Uninstallation

1. Delete the program installation directory
2. Delete user data directory (if any)
3. Clean registry entries (if any)

### MSI Installer Package Uninstallation

1. Control Panel → Programs and Features
2. Find HonyWing and uninstall

## Technical Support

If you encounter problems, please:

1. Check log files (logs/ directory)
2. Check system event logs
3. Submit an Issue to the project repository
4. Contact the development team

## Version History

- v1.0.0: Initial version, supports basic image matching and mouse simulation features
- Subsequent versions will be updated here

---

**Note**: This document will be continuously maintained as the project updates. Please refer to the latest version.

**@author: Mr.Rey Copyright © 2025**
**@created: 2025-01-07**
**@version: 1.0.0**
