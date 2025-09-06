# Contributing Guide

Thank you for your interest in the HonyWing project! We welcome all forms of contributions, including but not limited to:

- 🐛 Bug reports
- 💡 Feature suggestions
- 📝 Documentation improvements
- 🔧 Code contributions
- 🧪 Test cases

## 🚀 Quick Start

### Development Environment Requirements

- **Operating System**: Windows 10 1903+ or Windows 11
- **Development Tools**: Visual Studio 2022 or Visual Studio Code
- **Runtime**: .NET 8.0 SDK
- **Version Control**: Git

### Environment Setup

1. **Clone Repository**

   ```bash
   git clone https://github.com/reyisok/HonyWing.git
   cd HonyWing
   ```

2. **Install Dependencies**

   ```bash
   dotnet restore
   ```

3. **Build Project**

   ```bash
   dotnet build
   ```

4. **Run Project**

   ```bash
   dotnet run --project src\HonyWing.UI\HonyWing.UI.csproj
   ```

## 📋 Contribution Process

### 1. Create Issue

Before starting to code, please create an Issue to describe the problem you want to solve or the feature you want to add:

- Use appropriate Issue templates
- Provide detailed descriptions and reproduction steps (for Bugs)
- Explain expected behavior and actual behavior
- Attach relevant screenshots or logs

### 2. Fork and Branch

1. **Fork the project** to your GitHub account
2. **Clone the forked repository** locally
3. **Create feature branch**:

   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b bugfix/issue-number
   ```

### 3. Development and Testing

- Follow project coding standards
- Write clear commit messages
- Add necessary test cases
- Ensure all tests pass
- Update relevant documentation

### 4. Submit Pull Request

1. **Push branch** to your Fork:

   ```bash
   git push origin feature/your-feature-name
   ```

2. **Create Pull Request**:
   - Use PR template
   - Link related Issues
   - Provide detailed change descriptions
   - Add test screenshots (if applicable)

## 📝 Coding Standards

### C# Code Standards

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use PascalCase for classes, methods, properties
- Use camelCase for local variables, parameters
- Use meaningful variable and method names
- Add appropriate XML documentation comments

### Code Structure

```csharp
/// <summary>
/// Example class description
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
    /// Example method description
    /// </summary>
    /// <param name="parameter">Parameter description</param>
    /// <returns>Return value description</returns>
    public async Task<Result> ExampleMethodAsync(string parameter)
    {
        // Implementation logic
    }
}
```

### XAML Standards

- Use consistent indentation (4 spaces)
- Arrange attributes by logical grouping
- Use resource dictionaries to manage styles
- Follow MVVM pattern

```xml
<UserControl x:Class="HonyWing.UI.Controls.ExampleControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <Button Content="Example Button"
                Style="{StaticResource PrimaryButtonStyle}"
                Command="{Binding ExampleCommand}" />
    </Grid>
</UserControl>
```

## 🧪 Testing Guide

### Unit Testing

- Use xUnit testing framework
- Test file naming: `ClassNameTests.cs`
- Test method naming: `MethodName_Scenario_ExpectedResult`

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

### Integration Testing

- Test complete user scenarios
- Use test data and mock services
- Verify UI interactions and business logic

## 📚 Documentation Contributions

### Documentation Types

- **API Documentation**: XML comments in code
- **User Documentation**: README.md and docs/ directory
- **Development Documentation**: Architecture design and technical specifications

### Documentation Standards

- Use Markdown format
- Provide clear example code
- Include necessary screenshots and diagrams
- Keep documentation synchronized with code updates

## 🐛 Bug Reports

### Report Template

Please use the following template to report bugs:

```markdown
## Bug Description
Briefly describe the problem encountered

## Reproduction Steps
1. Open the application
2. Click...
3. Enter...
4. Observe the error

## Expected Behavior
Describe what you expected to happen

## Actual Behavior
Describe what actually happened

## Environment Information
- Operating System: Windows 11
- .NET Version: 8.0
- Application Version: 1.0.0

## Additional Information
- Error logs
- Screenshots
- Other relevant information
```

## 💡 Feature Suggestions

### Suggestion Template

```markdown
## Feature Description
Briefly describe the suggested feature

## Use Cases
Describe when this feature would be needed

## Solution
Describe your proposed implementation approach

## Alternatives
Describe other possible implementation approaches

## Additional Information
- Related screenshots or prototypes
- Reference materials
```

## 🔍 Code Review

### Review Points

- **Functionality**: Does the code implement the expected functionality
- **Readability**: Is the code clear and understandable
- **Performance**: Are there any performance issues
- **Security**: Are there any security vulnerabilities
- **Testing**: Is there sufficient test coverage

### Review Process

1. Automated checks (CI/CD)
2. Code review (at least one maintainer)
3. Test verification
4. Documentation update confirmation

## 📞 Contact

If you have any questions or suggestions, you can contact us through:

- **GitHub Issues**: [Project Issues Page]
- **GitHub Discussions**: [Project Discussion Area]
- **Email**: [your-email@example.com]

## 📄 License

By contributing code, you agree that your contributions will be released under the same license as the project. Please ensure you have the right to contribute the code you submit.

---

Thank you again for your contribution to the HonyWing project! 🎉
