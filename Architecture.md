
# JobHunter - Software Architecture

## 1. Overview

JobHunter is a modular .NET 10 application designed using Clean Architecture.

The architecture separates business rules, application workflows, infrastructure services, HTTP endpoints, and user interface concerns.

The primary objectives are:

- Separation of concerns.
- Testability.
- Maintainability.
- Extensibility.
- Reduced coupling between components.
- Independent development of external integrations.

## 2. Architectural Style

The system uses Clean Architecture with the following projects:

| Project | Responsibility |
|---|---|
| JobHunter.Domain | Core business model |
| JobHunter.Application | Application use cases |
| JobHunter.Infrastructure | Persistence and external services |
| JobHunter.API | HTTP API and composition root |
| JobHunter.Presentation | Blazor user interface |
| JobHunter.Test | Automated tests |

## 3. Dependency Rules

Dependencies must point toward the core business logic.

### Domain

The Domain project must not reference other JobHunter projects.

### Application

The Application project references Domain.

### Infrastructure

Infrastructure references Application and Domain.

It implements interfaces defined by Application.

### API

API references Application and Infrastructure.

The API project configures dependency injection and exposes application functionality through HTTP.

### Presentation

Presentation communicates with API through HTTP.

The initial design does not require a direct project reference to the API or Infrastructure projects.

### Tests

The test project references the layers under test.

Additional references may be added for integration tests.

## 4. Component Responsibilities

### 4.1 Domain Layer

Contains:

- JobPosting
- JobApplication
- JobSearchProfile
- ApplicationStatus
- JobSourceType
- Domain exceptions and business rules

The Domain layer must not contain:

- EF Core DbContext implementations.
- HTTP clients.
- Controllers.
- Blazor components.
- Database connection strings.

### 4.2 Application Layer

Contains:

- DTOs.
- Service interfaces.
- Repository interfaces.
- Application services.
- Commands.
- Queries.
- Validation logic.

The Application layer coordinates business workflows without depending on specific storage or external API implementations.

### 4.3 Infrastructure Layer

Contains:

- Entity Framework Core DbContext.
- Database entity configurations.
- Repository implementations.
- Database migrations.
- External job provider clients.
- Background job implementations.
- External HTTP integrations.

### 4.4 API Layer

Contains:

- REST controllers.
- Request handling.
- Dependency injection configuration.
- Middleware.
- Error handling.
- API documentation configuration.

Controllers should remain thin and delegate business operations to application services.

### 4.5 Presentation Layer

Contains:

- Blazor pages.
- Reusable UI components.
- API client services.
- View models.
- Layout components.
- Static assets.

The Presentation layer must not directly query the database.

## 5. Request Processing Flow

A typical request follows this sequence:

1. The user interacts with a Blazor page.
2. The Presentation layer sends an HTTP request.
3. The API controller receives the request.
4. The controller calls an Application service.
5. The Application service validates and processes the request.
6. Repository interfaces are used for persistence.
7. Infrastructure implements repository operations.
8. SQL Server stores or retrieves the data.
9. The API returns an HTTP response.
10. Blazor updates the user interface.

## 6. Dependency Injection

Dependency injection is configured in the API project.

Example service registrations:

```csharp
builder.Services.AddScoped<IJobService, JobService>();

builder.Services.AddScoped<IJobRepository, JobRepository>();
```

These registrations are illustrative and require the corresponding classes and namespaces.

Infrastructure registration may later be encapsulated in an AddInfrastructure extension method.

## 7. Background Processing

Scheduled job collection will run independently of the Blazor user interface.

Planned responsibilities:

- Trigger searches twice daily.
- Load configured search profiles.
- Call enabled job providers.
- Normalize external responses.
- Validate advertisements.
- Detect duplicates.
- Save new advertisements.
- Record execution results.

A .NET BackgroundService may be used for the initial implementation.

For production deployment, scheduling must account for application restarts, time zones, duplicate execution, and concurrent workers.

## 8. External Provider Architecture

Each provider shall implement a shared abstraction.

Example:

```csharp
public interface IJobProvider
{
    Task<IReadOnlyList<ExternalJobDto>> SearchAsync(
        JobSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}
```

This is a proposed interface, not an existing implementation.

Each provider implementation shall be responsible for:

- Authentication where required.
- Request construction.
- Response parsing.
- Error handling.
- Rate-limit handling.
- Data normalization.

## 9. Persistence

The application uses Entity Framework Core with SQL Server.

The Infrastructure project owns:

- DbContext.
- Entity configurations.
- Migrations.
- Repository implementations.

The Application layer accesses persistence through interfaces.

## 10. Error Handling

The system should distinguish between:

- Validation failures.
- Missing resources.
- Duplicate advertisements.
- External provider failures.
- Database failures.
- Unexpected internal errors.

The API should return consistent HTTP error responses.

Sensitive implementation details must not be exposed to clients.

## 11. Security

Security requirements include:

- HTTPS.
- Input validation.
- Safe external URL handling.
- Secure configuration storage.
- Restricted database permissions.
- Logging without sensitive credentials.
- Authentication before exposing personal application data outside a trusted local environment.

## 12. Architectural Decisions

### ADR-001: Clean Architecture

Decision: Separate Domain, Application, Infrastructure, API, and Presentation.

Reason: Maintainability and testability.

### ADR-002: ASP.NET Core Web API

Decision: Expose backend operations through REST endpoints.

Reason: Clear separation between backend and frontend.

### ADR-003: Blazor Web App

Decision: Use Blazor for the initial web interface.

Reason: Shared C# development ecosystem.

### ADR-004: SQL Server

Decision: Use SQL Server as the primary relational database.

Reason: EF Core integration and relational data support.

### ADR-005: External Provider Abstractions

Decision: Isolate provider-specific logic behind interfaces.

Reason: External APIs may change independently.

## 13. Future Architecture Improvements

- Authentication and authorization.
- Distributed background processing.
- Retry policies.
- Provider-specific rate limiting.
- Application event history.
- Observability dashboards.
- Containerized deployment.
- Multi-user support.
