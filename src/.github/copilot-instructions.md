# Copilot Instructions

## Project Guidelines
- Do not automatically create markdown documentation files, README files, or helper scripts unless explicitly requested. Only make the specific code changes asked for.
- Prefer warning fixes without adding new methods; keep changes minimal and in-place.
- Do not automatically perform git commits or pushes; only commit or push when explicitly requested.

## Code Style Guidelines
- Use primary constructors for dependency injection, but capture parameters into `readonly` fields to preserve immutability. For example:
  ```csharp
  public class MyService(IRepository repository)
  {
      private readonly IRepository _repository = repository;
  }
  ```

## Testing Guidelines
- When writing tests, use variable references in assertions instead of hardcoded values. For example, use `result.Name.Should().Be(request.Name)` instead of `result.Name.Should().Be("Test Name")`. This makes tests more maintainable - if test input values change, assertions automatically use the updated values.
- Always use `Bogus.Faker` to generate data in tests unless the test requires the data to be created manually or with specific values.
- Prefer test helper classes to be public when used across test files (e.g., MediaTypes helper should be public).