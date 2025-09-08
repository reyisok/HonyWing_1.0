# HonyWing - 智能鼠标自动化工具

<div align="center">

![License](https://img.shields.io/badge/license-MIT%20Modified-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows%2011-blue.svg)
![Language](https://img.shields.io/badge/language-C%23-green.svg)

一个基于图像识别的智能鼠标自动化工具，专为 Windows 11 系统优化设计。

</div>

## 近期变动

- 自定义区域不生效(修复中)
- 优化：改进了图像匹配算法，提高了匹配准确率(已实现)
- 修复：解决了在高分辨率显示器下的显示问题(已实现)
- 支持多张图片参照，可以对监控区域多个参照目标进行监控(计划中)

## ✨ 功能特性

### 🎯 核心功能

- **智能图像匹配**：基于 OpenCV 的高精度模板匹配算法
- **屏幕区域监控**：支持全屏、窗口、自定义区域监控
- **鼠标模拟操作**：精确的鼠标点击、移动、拖拽模拟
- **多目标检测**：同时监控多个目标图像，智能排序匹配
- **实时状态监控**：详细的运行日志和匹配记录

### 🔧 高级特性

- **DPI 自适应**：完美支持高分辨率显示器和多显示器环境
- **点击动画效果**：可视化点击位置，提供直观反馈
- **配置管理**：支持配置文件的保存、加载和导入导出
- **快捷键操作**：丰富的快捷键支持，提升操作效率
- **托盘运行**：支持最小化到系统托盘，后台静默运行

### 🎨 用户体验

- **现代化界面**：采用 ModernWpf 设计，符合 Windows 11 设计语言
- **深蓝色主题**：统一的视觉风格，护眼舒适
- **响应式布局**：适配不同屏幕尺寸和 DPI 设置
- **直观操作**：拖拽上传、区域选择、一键启停

## 🚀 快速开始

### 系统要求

- **操作系统**：Windows 11 (推荐) 或 Windows 10 1903+
- **运行时**：.NET 8.0 Runtime
- **内存**：至少 512MB 可用内存
- **显示器**：支持任意分辨率和 DPI 设置

### 安装步骤

1. **下载发布版本**

   ```bash
   # 从 Releases 页面下载最新版本
   # 或克隆源代码自行编译
   git clone https://github.com/reyisok/HonyWing_1.0.git
   ```

2. **编译运行**（开发者）

   ```bash
   cd HonyWing
   dotnet restore
   dotnet build
   dotnet run --project src\HonyWing.UI\HonyWing.UI.csproj
   ```

3. **首次使用**
   - 启动应用程序
   - 上传目标图像（支持 PNG、JPG、BMP 格式）
   - 设置监控区域
   - 配置匹配参数
   - 点击"开始匹配"按钮

## 📖 使用指南

### 基本操作流程

1. **图像管理**

   - 点击"选择图片"按钮或拖拽图片到上传区域
   - 在预览区查看和调整目标图像
   - 支持多图像管理，可添加、删除、排序

2. **区域设置**

   - 选择监控模式：全屏、当前窗口、自定义区域
   - 使用"选择区域"工具精确框选监控范围
   - 实时预览选中区域

3. **参数配置**

   - **匹配精度**：调整相似度阈值（0.1-1.0）
   - **点击延迟**：设置点击间隔时间
   - **点击类型**：左键、右键、双击
   - **高级选项**：平滑移动、点击动画等

4. **运行控制**
   - **开始匹配**：启动自动监控和点击
   - **暂停/继续**：临时暂停或恢复运行
   - **停止**：完全停止监控
   - **快捷键**：Space（暂停/继续）、Esc（停止）

### 高级功能

#### DPI 适配

- 自动检测系统 DPI 设置
- 支持 100%、125%、150%、200% 等常见缩放比例
- 多显示器环境下的智能坐标转换

#### 配置管理

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

## 🏗️ 技术架构

### 项目结构

```
HonyWing/
├── src/
│   ├── HonyWing.Core/           # 核心业务逻辑
│   │   ├── Interfaces/          # 接口定义
│   │   ├── Models/              # 数据模型
│   │   └── Services/            # 业务服务
│   ├── HonyWing.Infrastructure/ # 基础设施层
│   │   └── Services/            # 基础服务实现
│   └── HonyWing.UI/            # WPF 用户界面
│       ├── Views/              # 视图
│       ├── ViewModels/         # 视图模型
│       ├── Controls/           # 自定义控件
│       ├── Converters/         # 值转换器
│       └── Styles/             # 样式资源
├── docs/                       # 项目文档
├── tools/                      # 开发工具
└── test/                       # 测试项目
```

### 技术栈

- **框架**：.NET 8.0 + WPF
- **语言**：C# 12.0
- **图像处理**：OpenCVSharp4
- **UI 库**：ModernWpf
- **依赖注入**：Microsoft.Extensions.DependencyInjection
- **日志系统**：NLog
- **配置管理**：System.Text.Json

### 核心组件

#### 图像匹配引擎

- 基于 OpenCV 模板匹配算法
- 支持多尺度匹配和旋转不变性
- 优化的匹配性能，4K 屏幕下 < 200ms

#### DPI 适配系统

- 实时 DPI 检测和坐标转换
- 支持动态 DPI 变更
- 多显示器环境适配

#### 鼠标模拟服务

- 基于 Windows API (user32.dll)
- 支持自然的鼠标移动轨迹
- 可配置的点击延迟和动画效果

## 🤝 贡献指南

我们欢迎社区贡献！请遵循以下步骤：

1. **Fork 项目**
2. **创建功能分支** (`git checkout -b feature/AmazingFeature`)
3. **提交更改** (`git commit -m 'Add some AmazingFeature'`)
4. **推送分支** (`git push origin feature/AmazingFeature`)
5. **创建 Pull Request**

### 开发规范

- 遵循 C# 编码规范
- 添加适当的单元测试
- 更新相关文档
- 确保代码通过所有测试

## 📄 开源许可

本项目采用修订版 MIT 许可证，详见 [LICENSE.txt](LICENSE.txt) 文件。

### 许可证要点

- ✅ 个人学习和研究使用
- ✅ 非商业性质的个人项目
- ❌ 商业用途需要书面许可
- ❌ 企业/组织内部使用需要授权

## 🙏 致谢

感谢以下开源项目的支持：

- [OpenCVSharp](https://github.com/shimat/opencvsharp) - 图像处理库
- [ModernWpf](https://github.com/Kinnara/ModernWpf) - 现代化 WPF UI 库
- [NLog](https://github.com/NLog/NLog) - 日志记录框架
- [Microsoft.Toolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM 工具包

## 📞 联系方式

- **作者**：Mr.Rey
- **邮箱**：[reyisok@live.com]
- **项目主页**：[https://github.com/reyisok/HonyWing_1.0]

## 致先驱者的致谢

向所有在技术领域披荆斩棘的先驱者致以最诚挚的谢意。作为一名初学者，正是承蒙站在你们的肩膀上，从前人的经验与探索中汲取养分，我才得以突破思路局限，将想法落地为 HonyWing 中各类用于学习实践的功能模块，让技术探索的乐趣有了承载。

特别感谢每一位使用 HonyWing 的用户。作为一款仅限个人学习用途的实验性系统，它的核心价值正来源于你们的体验与反馈 —— 无论是对屏幕监控、颜色识别、文字识别还是图像匹配功能的使用建议，亦或是在学习模拟操作、测试识别过程中发现的问题，这些真实反馈都成为了系统优化的重要方向，也让我这个初学者更清晰地理解 “技术服务于学习” 的核心目标。

需要说明的是，本项目目前仍处于完善阶段，功能打磨与体验优化仍在持续推进中。作为初学者，我在开发过程中难免有考虑不周之处，若大家在使用时遇到操作异常、识别偏差等问题，恳请多予包容，也欢迎随时提出改进意见，你们的包容与建议，会成为我成长和项目完善的重要助力。

在整个项目的学习与开发过程中，我更深刻地体会到 “规范” 二字的重量。一套清晰的代码规范、成熟的开发最佳实践，不仅让我这个初学者能更轻松地梳理代码逻辑，更为项目后续的维护与功能扩展扫清了障碍。那些经过实践验证的规范指导，如同技术道路上的灯塔，让我少走了许多弯路，也让这个实验性项目始终保持着可迭代、可优化的活力。

最后，愿每一位探索者都能在技术学习中收获乐趣，Enjoy your lucky day！

Mr. Rey
By HonyWing 团队（包含 AI 助手）

---

<div align="center">

**如果这个项目对您有帮助，请考虑给它一个 ⭐**

</div>
