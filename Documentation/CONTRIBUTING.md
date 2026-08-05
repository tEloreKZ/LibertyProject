# Contributing

**Contribution Guidelines for Project Liberty**

---

## Table of Contents

1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Development Workflow](#development-workflow)
4. [Code Review Process](#code-review-process)
5. [Submitting Changes](#submitting-changes)
6. [Reporting Issues](#reporting-issues)
7. [Coding Standards](#coding-standards)

---

## Overview

Thank you for your interest in contributing to Project Liberty! This document provides guidelines for contributing to the project.

### Contribution Areas

- **Core Systems**: Bug fixes, performance improvements
- **Documentation**: Improvements to existing docs, new docs
- **Plugins**: New callouts, custom systems
- **Translations**: New language support, translation improvements
- **Testing**: Unit tests, integration tests

---

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Git
- GTA V with ScriptHookVDotNet (for testing)

### Setting Up Development Environment

1. **Clone the repository**

```bash
git clone https://github.com/your-org/ProjectLiberty.git
cd ProjectLiberty
```

2. **Restore dependencies**

```bash
dotnet restore
```

3. **Build the solution**

```bash
dotnet build
```

4. **Run tests**

```bash
dotnet test
```

---

## Development Workflow

### Branch Strategy

- **main**: Stable release branch
- **develop**: Development branch
- **feature/***: Feature branches
- **bugfix/***: Bug fix branches
- **hotfix/***: Hot fix branches

### Creating a Feature Branch

```bash
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name
```

### Making Changes

1. **Follow coding standards** (see CODING_STANDARDS.md)
2. **Write tests** for new functionality
3. **Update documentation** as needed
4. **Commit changes** with clear messages

### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Test changes
- `chore`: Build process or tool changes

**Example:**
```
feat(dispatch): add radio channel switching

Implemented radio channel switching functionality with
support for 3 channels and encryption.

Closes #123
```

---

## Code Review Process

### Before Submitting

1. **Run tests**: Ensure all tests pass
2. **Check formatting**: Follow coding standards
3. **Update docs**: Update relevant documentation
4. **Self-review**: Review your own changes

### Pull Request Process

1. **Create pull request** from feature branch to develop
2. **Fill PR template** with description
3. **Request review** from maintainers
4. **Address feedback** from reviewers
5. **Merge** after approval

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
Describe testing performed

## Checklist
- [ ] Code follows project standards
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No breaking changes (or documented)
```

---

## Submitting Changes

### Pull Request Guidelines

- **Small, focused PRs**: One feature or bug fix per PR
- **Clear descriptions**: Explain what and why
- **Tests included**: New features must have tests
- **Documentation updated**: Docs must reflect changes
- **No merge conflicts**: Resolve conflicts before submitting

### Plugin Contributions

For plugin contributions:

1. **Follow plugin SDK** guidelines
2. **Include manifest** with plugin metadata
3. **Provide translations** for supported languages
4. **Include configuration** schema
5. **Test thoroughly** in game environment

---

## Reporting Issues

### Bug Reports

Include:

- **Description**: Clear description of the bug
- **Steps to reproduce**: Exact steps to reproduce
- **Expected behavior**: What should happen
- **Actual behavior**: What actually happens
- **Environment**: Game version, mod version, etc.
- **Logs**: Relevant log files

### Feature Requests

Include:

- **Description**: Clear description of the feature
- **Use case**: Why this feature is needed
- **Alternatives**: Alternative solutions considered
- **Priority**: Low, medium, high

---

## Coding Standards

### General Rules

- Follow CODING_STANDARDS.md
- Use XML documentation for public APIs
- No hardcoded strings (use localization)
- No direct native calls (use abstractions)
- Follow SOLID principles

### Code Style

- Use PascalCase for classes, methods, properties
- Use camelCase for local variables, parameters
- Use `_camelCase` for private fields
- Use meaningful names
- Keep methods short and focused

### Testing

- Write unit tests for all public methods
- Aim for 80%+ code coverage
- Use descriptive test names
- Test edge cases and error conditions

---

## Community Guidelines

### Code of Conduct

- Be respectful and inclusive
- Welcome newcomers and help them learn
- Focus on constructive feedback
- Assume good intentions
- Be patient with different perspectives

### Communication

- Use clear, professional language
- Be concise and to the point
- Ask questions when unsure
- Share knowledge freely
- Give credit where due

---

## Recognition

Contributors will be recognized in:

- CONTRIBUTORS.md file
- Release notes
- Project documentation

---

## License

By contributing to Project Liberty, you agree that your contributions will be licensed under the project's license (see LICENSE.md).

---

## Contact

- **Issues**: GitHub Issues
- **Discussions**: GitHub Discussions
- **Discord**: [Discord Server Link]

---

## Summary

Contributing to Project Liberty requires:

- **Following coding standards** and guidelines
- **Writing tests** for all changes
- **Updating documentation** as needed
- **Participating in code review** process
- **Being respectful** and collaborative

Thank you for contributing to Project Liberty!
