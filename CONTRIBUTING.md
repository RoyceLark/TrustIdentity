# Contributing to TrustIdentity

Thank you for your interest in contributing to TrustIdentity!

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment.

## How to Contribute

### Reporting Issues

1. Check existing issues first
2. Use the issue template
3. Provide detailed reproduction steps
4. Include environment information

### Submitting Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Update documentation
7. Commit with clear messages (`git commit -m 'Add amazing feature'`)
8. Push to your fork (`git push origin feature/amazing-feature`)
9. Open a Pull Request

## Development Setup

```bash
# Clone repository
git clone https://github.com/roycelark/trustidentity.git
cd trustidentity

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test
```

## Coding Standards

- Follow C# coding conventions
- Use meaningful variable names
- Add XML documentation comments
- Keep methods focused and small
- Write unit tests for new code
- Maintain backwards compatibility

## Testing

- Write unit tests for all new features
- Maintain >80% code coverage
- Include integration tests where appropriate
- Test edge cases and error conditions

## Documentation

- Update README.md for new features
- Add XML comments to public APIs
- Update CHANGELOG.md
- Include code examples

## License

By contributing, you agree that your contributions will be licensed under the Apache License 2.0.

## Questions?

Open an issue or discussion on GitHub.