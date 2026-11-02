
# JobHunter - Development Guide

## 1. Purpose

This guide describes the development environment, coding conventions, solution structure, and recommended workflow for the JobHunter project.

## 2. Development Environment

Required tools:

- Visual Studio 2026.
- .NET 10 SDK.
- Git.
- SQL Server or SQL Server LocalDB.
- SQL Server management tools, optional.

Recommended Visual Studio workloads:

- ASP.NET and web development.
- .NET desktop development, if needed for additional tools.

## 3. Solution Structure

The solution contains six projects:

- JobHunter.Domain
- JobHunter.Application
- JobHunter.Infrastructure
- JobHunter.API
- JobHunter.Presentation
- JobHunter.Test

The Test project may be renamed to JobHunter.Tests later if desired, but project references and namespaces must remain consistent.

## 4. Project Dependencies

| Project | References |
|---|---|
| Domain | None |
| Application | Domain |
| Infrastructure | Application, Domain |
| API | Application, Infrastructure |
| Presentation | None |
| Test | Domain, Application |

The test project may reference Infrastructure or API when implementing integration tests.

## 5. NuGet Packages

### Infrastructure

Planned packages:

- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Design, if required by the selected tooling setup

### API

Planned packages:

- Microsoft.EntityFrameworkCore.Design, if API is the EF Core startup project
- OpenAPI packages required by the selected API template

### Test

- Microsoft.NET.Test.Sdk
- xunit
- xunit.runner.visualstudio
- coverlet.collector, optional

Use stable package versions compatible with .NET 10.

Keep EF Core package versions consistent.

## 6. Building the Solution

From the directory containing the solution file:

```powershell
dotnet restore
dotnet build
```

Run tests:

```powershell
dotnet test
```

Alternatively, use Visual Studio:

Build > Build Solution

## 7. Running the Application

### Backend

Set JobHunter.API as the startup project.

Start the API using Visual Studio or:

```powershell
dotnet run --project path/to/JobHunter.API.csproj
```

Replace the project path with the actual location.

### Frontend

Set JobHunter.Presentation as the startup project.

The Blazor application will communicate with the backend through HTTP.

Configure the API base address before using API-dependent pages.

### Multiple Startup Projects

In Visual Studio, configure the Solution to start both API and Presentation when needed.

## 8. Coding Conventions

### Naming

- Classes: PascalCase.
- Methods: PascalCase.
- Public properties: PascalCase.
- Local variables: camelCase.
- Private fields: _camelCase.
- Interfaces: Prefix with I.

Examples:

```csharp
public interface IJobRepository
{
}

public sealed class JobService
{
    private readonly IJobRepository _repository;
}
```

### General Rules

- Enable nullable reference types.
- Prefer constructor injection.
- Use asynchronous I/O.
- Accept CancellationToken where appropriate.
- Avoid unnecessary static state.
- Keep controllers small.
- Keep business logic out of the Presentation layer.
- Avoid direct database access from controllers.

## 9. Domain Development

Domain entities should encapsulate business rules.

Avoid unrestricted property setters when they would allow invalid entity states.

Use explicit methods for important state transitions.

Example:

```csharp
public void ChangeStatus(ApplicationStatus newStatus)
{
    if (!Enum.IsDefined(newStatus))
    {
        throw new ArgumentOutOfRangeException(nameof(newStatus));
    }

    Status = newStatus;
}
```

This example assumes an entity with a Status property.

## 10. Application Development

Application services should:

1. Receive input.
2. Validate application-level requirements.
3. Execute domain operations.
4. Use repository abstractions.
5. Return DTOs or operation results.

Application services should not depend directly on EF Core DbContext implementations.

## 11. Infrastructure Development

Infrastructure is responsible for technical implementations.

Examples:

- SQL Server persistence.
- External API clients.
- Scheduled background services.
- HTTP retry handling.
- Logging integrations.

## 12. API Development

Use REST conventions.

Recommended controller structure:

```text
Controllers/
├── JobsController.cs
├── SearchProfilesController.cs
└── SearchController.cs
```

Controller actions should delegate operations to Application services.

## 13. Frontend Development

The Presentation project uses Blazor.

Recommended structure:

```text
Components/
├── Layout/
├── Pages/
└── Shared/

Services/
Models/
wwwroot/
```

Use typed HttpClient services for API communication.

Avoid mixing database logic into Razor components.

## 14. Git Workflow

Recommended branch names:

- main
- feature/job-management
- feature/search-providers
- feature/application-tracking
- fix/duplicate-detection

Typical workflow:

```powershell
git status
git add .
git commit -m "Add job posting domain model"
git push
```

Do not commit:

- Database passwords.
- API keys.
- User secrets.
- Local database files.
- Build output.
- Temporary files.

## 15. Documentation Workflow

Update documentation when:

- New functionality is introduced.
- API contracts change.
- Database entities change.
- Architecture decisions change.
- Tests or deployment requirements change.

## 16. Definition of Done

A feature is complete when:

1. Requirements are satisfied.
2. The solution builds.
3. Relevant tests pass.
4. Errors are handled.
5. No credentials are exposed.
6. Documentation is updated.
7. Changes are committed to Git.
