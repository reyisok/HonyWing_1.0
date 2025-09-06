# 贡献指南

感谢您对 HonyWing 项目的关注！我们欢迎各种形式的贡献，包括但不限于：

- 🐛 Bug 报告
- 💡 功能建议
- 📝 文档改进
- 🔧 代码贡献
- 🧪 测试用例

## 🚀 快速开始

### 开发环境要求

- **操作系统**: Windows 10 1903+ 或 Windows 11
- **开发工具**: Visual Studio 2022 或 Visual Studio Code
- **运行时**: .NET 8.0 SDK
- **版本控制**: Git

### 环境搭建

1. **克隆仓库**

   ```bash
   git clone https://github.com/reyisok/HonyWing.git
   cd HonyWing
   ```

2. **安装依赖**

   ```bash
   dotnet restore
   ```

3. **构建项目**

   ```bash
   dotnet build
   ```

4. **运行项目**

   ```bash
   dotnet run --project src\HonyWing.UI\HonyWing.UI.csproj
   ```

## 📋 贡献流程

### 1. 创建 Issue

在开始编码之前，请先创建一个 Issue 来描述您要解决的问题或添加的功能：

- 使用合适的 Issue 模板
- 提供详细的描述和重现步骤（对于 Bug）
- 说明预期的行为和实际行为
- 附上相关的截图或日志

### 2. Fork 和分支

1. **Fork 项目**到您的 GitHub 账户
2. **克隆 Fork 的仓库**到本地
3. **创建功能分支**：

   ```bash
   git checkout -b feature/your-feature-name
   # 或
   git checkout -b bugfix/issue-number
   ```

### 3. 开发和测试

- 遵循项目的编码规范
- 编写清晰的提交信息
- 添加必要的测试用例
- 确保所有测试通过
- 更新相关文档

### 4. 提交 Pull Request

1. **推送分支**到您的 Fork：

   ```bash
   git push origin feature/your-feature-name
   ```

2. **创建 Pull Request**：
   - 使用 PR 模板
   - 链接相关的 Issue
   - 提供详细的变更说明
   - 添加测试截图（如适用）

## 📝 编码规范

### C# 代码规范

- 遵循 [Microsoft C# 编码约定](https://docs.microsoft.com/zh-cn/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- 使用 PascalCase 命名类、方法、属性
- 使用 camelCase 命名局部变量、参数
- 使用有意义的变量和方法名
- 添加适当的 XML 文档注释

### 代码结构

```csharp
/// <summary>
/// 示例类说明
/// </summary>
/// <author>Mr.Rey Copyright © 2025</author>
/// <created>2025-01-13</created>
/// <modified>2025-01-13</modified>
/// <version>1.0.0</version>
public class ExampleClass
{
    private readonly IService _service;

    public ExampleClass(IService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// 示例方法说明
    /// </summary>
    /// <param name="parameter">参数说明</param>
    /// <returns>返回值说明</returns>
    public async Task<Result> ExampleMethodAsync(string parameter)
    {
        // 实现逻辑
    }
}
```

### XAML 规范

- 使用一致的缩进（4个空格）
- 属性按逻辑分组排列
- 使用资源字典管理样式
- 遵循 MVVM 模式

```xml
<UserControl x:Class="HonyWing.UI.Controls.ExampleControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <Button Content="示例按钮"
                Style="{StaticResource PrimaryButtonStyle}"
                Command="{Binding ExampleCommand}" />
    </Grid>
</UserControl>
```

## 🧪 测试指南

### 单元测试

- 使用 xUnit 测试框架
- 测试文件命名：`ClassNameTests.cs`
- 测试方法命名：`MethodName_Scenario_ExpectedResult`

```csharp
[Fact]
public void CalculateDistance_ValidCoordinates_ReturnsCorrectDistance()
{
    // Arrange
    var point1 = new Point(0, 0);
    var point2 = new Point(3, 4);

    // Act
    var distance = GeometryHelper.CalculateDistance(point1, point2);

    // Assert
    Assert.Equal(5.0, distance, 2);
}
```

### 集成测试

- 测试完整的用户场景
- 使用测试数据和模拟服务
- 验证 UI 交互和业务逻辑

## 📚 文档贡献

### 文档类型

- **API 文档**: 代码中的 XML 注释
- **用户文档**: README.md 和 docs/ 目录
- **开发文档**: 架构设计和技术说明

### 文档规范

- 使用 Markdown 格式
- 提供清晰的示例代码
- 包含必要的截图和图表
- 保持文档与代码同步更新

## 🐛 Bug 报告

### 报告模板

请使用以下模板报告 Bug：

```markdown
## Bug 描述
简要描述遇到的问题

## 重现步骤
1. 打开应用程序
2. 点击...
3. 输入...
4. 观察到错误

## 预期行为
描述您期望发生的情况

## 实际行为
描述实际发生的情况

## 环境信息
- 操作系统: Windows 11
- .NET 版本: 8.0
- 应用版本: 1.0.0

## 附加信息
- 错误日志
- 截图
- 其他相关信息
```

## 💡 功能建议

### 建议模板

```markdown
## 功能描述
简要描述建议的功能

## 使用场景
描述什么情况下需要这个功能

## 解决方案
描述您认为的实现方式

## 替代方案
描述其他可能的实现方式

## 附加信息
- 相关截图或原型
- 参考资料
```

## 🔍 代码审查

### 审查要点

- **功能性**: 代码是否实现了预期功能
- **可读性**: 代码是否清晰易懂
- **性能**: 是否存在性能问题
- **安全性**: 是否存在安全隐患
- **测试**: 是否有足够的测试覆盖

### 审查流程

1. 自动化检查（CI/CD）
2. 代码审查（至少一位维护者）
3. 测试验证
4. 文档更新确认

## 📞 联系方式

如果您有任何问题或建议，可以通过以下方式联系我们：

- **GitHub Issues**: [项目 Issues 页面]
- **GitHub Discussions**: [项目讨论区]
- **邮箱**: [your-email@example.com]

## 📄 许可证

通过贡献代码，您同意您的贡献将在与项目相同的许可证下发布。请确保您有权贡献您提交的代码。

---

再次感谢您对 HonyWing 项目的贡献！🎉
