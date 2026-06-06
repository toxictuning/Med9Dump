# Contributing to Med9Dump

Thank you for considering contributing to Med9Dump! This document provides guidelines and instructions for contributions.

## Code of Conduct

Be respectful, professional, and constructive in all interactions. This project welcomes contributors from all backgrounds.

## Getting Started

### Prerequisites

- Windows 7 or later
- .NET Framework 4.8 SDK
- Visual Studio 2022 (Community Edition or higher)
- Git

### Setting Up Your Development Environment

1. **Fork the repository** on GitHub

2. **Clone your fork locally**:
   ```bash
   git clone https://github.com/YOUR-USERNAME/Med9Dump.git
   cd Med9Dump
   ```

3. **Add upstream remote** (to stay in sync):
   ```bash
   git remote add upstream https://github.com/toxictuning/Med9Dump.git
   ```

4. **Open the solution** in Visual Studio 2022:
   - File > Open > Med9Dump.sln
   - Verify target framework is `.NET Framework 4.8`

5. **Build the project**:
   - Build > Build Solution (Ctrl+Shift+B)

### Running Tests

Currently, this project does not have automated tests. Manual testing should be performed on real firmware images.

## Submitting Changes

### Before You Start

1. **Create an issue** (if one doesn't exist) describing what you want to fix or add
   - Reference existing issues to avoid duplication
   - For bugs, include: steps to reproduce, expected behavior, actual behavior

2. **Discuss major changes** before implementing
   - Comment on the issue to propose your solution
   - Wait for feedback from maintainers

### Making Changes

1. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```
   or
   ```bash
   git checkout -b fix/your-bug-description
   ```

2. **Write clear, idiomatic code**:
   - Follow C# naming conventions (PascalCase for public members, camelCase for local variables)
   - Use meaningful variable and function names
   - Keep methods focused and reasonably short
   - Add comments only when behavior is non-obvious

3. **Respect existing code style**:
   - Match indentation (4 spaces)
   - Follow the existing pattern for error handling
   - Use LINQ appropriately but not excessively
   - Keep compatibility with .NET Framework 4.8 (no newer C# features)

4. **Add necessary documentation**:
   - Update `README.md` if adding new features or changing usage
   - Update code comments if changing complex logic
   - Include example usage in commit messages when relevant

### Example: Coding Style

**Good:**
```csharp
static bool HasValidRom(byte[] data, int expectedSize)
{
    return data != null && data.Length >= expectedSize;
}

// Later in code
if (!HasValidRom(rom, CodeOffset + CodeLength))
{
    Console.Error.WriteLine("Error: ROM is too small");
    return 1;
}
```

**Avoid:**
```csharp
// Unclear variable names
bool b = r != null && r.Length >= s;

// Inconsistent error handling
if (b) { /* ... */ }
else Console.WriteLine("Error");

// Overly complex one-liners
var results = data.Where(x => x.Address.Substring(2).All(c => "0123456789ABCDEFabcdef".Contains(c))).ToList();
```

## Submitting Pull Requests

1. **Update your branch** with latest upstream:
   ```bash
   git fetch upstream
   git rebase upstream/master
   ```

2. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

3. **Open a Pull Request** on GitHub:
   - Reference the issue: "Fixes #123" or "Addresses #456"
   - Write a clear title: "Add feature X" or "Fix bug in Y"
   - Describe what changed and why
   - Include any relevant context or examples

4. **Pull Request Checklist**:
   - [ ] Code builds without errors
   - [ ] No regressions in existing functionality
   - [ ] Changes follow code style guidelines
   - [ ] Documentation is updated if needed
   - [ ] Commit messages are clear and descriptive
   - [ ] No unnecessary files or debug code included

### PR Title Format

- ? "Add PowerPC instruction validation"
- ? "Fix incorrect pointer table offset parsing"
- ? "Update README with new CSV format example"
- ? "Fix bug"
- ? "Updates"

## Legal and Licensing

### Important Constraints

This project is licensed under the **MIT License**. By contributing, you agree that:

1. Your contributions will also be licensed under the MIT License
2. You have the right to contribute the code (original work or properly licensed)
3. Your contribution does not violate any third-party rights
4. You assume responsibility for the legality of your contributions

### Third-Party Code

- Do not include code from other projects without proper attribution
- If you use external libraries, ensure they are compatible with MIT License
- Update `licenses/` directory and `README.md` if adding dependencies

## Reporting Bugs

### Security Vulnerabilities

?? **Do not open a public issue for security vulnerabilities.**

Email security concerns directly to the maintainer instead of posting on GitHub.

### Regular Bugs

1. **Check existing issues** first to avoid duplicates

2. **Describe the bug clearly**:
   - **Title**: One-line summary (e.g., "Incorrect pointer calculation when TKMWL_Size > 256")
   - **Steps to reproduce**: Exact steps and input files
   - **Expected behavior**: What should happen
   - **Actual behavior**: What actually happens
   - **Environment**: Windows version, .NET Framework version, how you built the project
   - **Error messages**: Include full stack traces or console output

3. **Provide example files** (if safe to do so):
   - Small, anonymized test cases are helpful
   - Do not commit copyrighted firmware files to the repository

4. **Example Bug Report**:
   ```
   Title: Disassembler crashes on zero-length input file

   Steps to reproduce:
   1. Create an empty file named 'empty.bin'
   2. Run: Med9Dump.exe empty.bin --print

   Expected: Graceful error message
   Actual: Unhandled exception (see attached log)

   Environment: Windows 10, .NET Framework 4.8, Debug build
   ```

## Improving Documentation

- Typos and clarity improvements are always welcome
- Update `README.md` to fix inconsistencies
- Improve code comments for complex sections
- Create examples in the `docs/` directory if needed

## Development Tips

### Debugging

- Use Visual Studio's debugger (F5 to start debugging)
- Set breakpoints and inspect variables
- Use conditional breakpoints for specific issues
- Log to console for quick diagnostics: `Console.WriteLine("Debug: " + variable);`

### Common Tasks

**Building Release Binaries**:
```bash
msbuild Med9Dump.sln /p:Configuration=Release /p:Platform=AnyCPU
```

**Cleaning Build Artifacts**:
```bash
git clean -fXd
```

**Running with Specific Parameters**:
```bash
./bin/Debug/Med9Dump.exe firmware.bin --print --show--allpointers
```

## Questions?

- Comment on an issue for help
- Check existing documentation in `README.md`
- Review past commit history for context

## Thank You!

Your contributions help make Med9Dump better. Whether it's bug fixes, features, documentation, or just feedback—it all helps! ??
