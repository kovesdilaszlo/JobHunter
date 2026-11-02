
# JobHunter - Software Architecture

## 1. Overview

JobHunter is a .NET 10 web application following Clean Architecture.

The system separates business rules, application workflows, infrastructure implementations, HTTP endpoints, and user interface components.

This document defines the planned architecture. Implementation has not started.

## 2. Architecture Goals

- Maintain clear separation of concerns.
- Keep business rules independent of external providers.
- Support automated testing.
- Allow additional job sources.
- Support reliable background execution.
- Prevent duplicate job advertisements.
- Enable future deployment using Docker.

## 3. Solution Projects

| Project | Responsibility |
|---|---|
| JobHunter.Domain | Business entities, enumerations, and rules |
| JobHunter.Application | Use cases, DTOs, interfaces, and services |
| JobHunter.Infrastructure | Persistence, providers, and background processing |
| JobHunter.API | REST endpoints and dependency injection |
| JobHunter.Presentation | Blazor web interface |
| JobHunter.Test | Automated tests |

## 4. Dependency Structure

### Domain

The Domain project shall have no dependencies on other JobHunter projects.

### Application

Application references Domain.

### Infrastructure

Infrastructure references Application and Domain.

### API

API references Application and Infrastructure.

### Presentation

Presentation communicates with API through HTTP.

### Tests

The test project references the projects required by each test category.

## 5. Domain Layer

The Domain layer contains the business model.

### Planned Entities

- JobPosting
- JobPostingSource
- JobApplication
- JobSearchProfile
- SearchExecution

SearchExecution may be implemented as a persistence-oriented model if it contains no meaningful domain behavior.

### Planned Enumerations

- ApplicationStatus
- JobSourceType
- WorkArrangement
- SearchExecutionStatus

### Responsibilities

- Validate entity state.
- Enforce business invariants.
- Manage application status transitions.
- Represent logical job advertisements.
- Represent multiple source references.

### Restrictions

Domain must not depend on:

- Entity Framework Core.
- HTTP clients.
- Controllers.
- Blazor.
- External API SDKs.
- Database implementations.

## 6. Application Layer

The Application layer coordinates use cases.

### Planned Interfaces

- IJobRepository
- IJobService
- IJobSearchProfileRepository
- IJobSearchProfileService
- IJobProvider
- IJobSearchService
- ISearchExecutionRepository
- IDuplicateDetectionService

The exact interface set may be refined during implementation.

### Planned Services

- JobService
- JobSearchProfileService
- JobSearchService
- DuplicateDetectionService
- ApplicationTrackingService

### Responsibilities

- Validate application requests.
- Coordinate repository operations.
- Execute search workflows.
- Normalize data into application models.
- Detect duplicates.
- Manage search profiles.
- Return DTOs.

The Application layer shall not directly access SQL Server or external HTTP endpoints.

## 7. Infrastructure Layer

Infrastructure implements technical dependencies.

### Persistence

Planned components:

- JobHunterDbContext.
- Entity configurations.
- EF Core migrations.
- Repository implementations.

### External Job Providers

Planned providers:

- JoobleJobProvider.
- ManualJobProvider.

LinkedIn and No Fluff Jobs advertisements will initially be supported through manual source links.

Automated provider implementations may be added if authorized integration methods become available.

### Background Processing

A .NET BackgroundService is the proposed initial scheduling mechanism.

Responsibilities:

- Check due searches.
- Execute enabled profiles.
- Respect API usage limits.
- Prevent overlapping searches.
- Record execution results.
- Handle cancellation and failures.

### Provider Integration Rules

Each automated provider shall:

- Use supported and authorized endpoints.
- Handle credentials securely.
- Respect rate limits.
- Normalize external data.
- Handle HTTP errors.
- Avoid uncontrolled retries.

## 8. Job Provider Abstraction

The Application layer will define a provider interface.

Conceptual example:

```csharp
public interface IJobProvider
{
    Task<IReadOnlyList<ExternalJobDto>> SearchAsync(
        JobSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}
```

The exact DTO and interface definitions will be finalized during implementation.

Provider-specific response formats shall not leak into the Domain layer.

## 9. Job Advertisement Model

A JobPosting represents one logical employment opportunity.

A JobPostingSource represents one external reference to that opportunity.

### Example

```text
JobPosting
    Id
    Title
    Company
    Location
    Description
    Status
    Sources
        JobPostingSource
            Provider: Jooble
            OriginalUrl: ...
        JobPostingSource
            Provider: LinkedIn
            OriginalUrl: ...
        JobPostingSource
            Provider: NoFluffJobs
            OriginalUrl: ...
```

One JobPosting may have multiple JobPostingSource records.

This avoids storing the same logical opportunity multiple times.

## 10. Duplicate Detection Architecture

Duplicate detection shall use multiple stages.

### Stage 1: Provider Identity

Compare provider name and external advertisement identifier.

A matching provider identity indicates a confirmed duplicate.

### Stage 2: Normalized URL

Normalize advertisement URLs and compare them.

Normalization may remove known tracking parameters and normalize URL formatting.

### Stage 3: Semantic Similarity

Compare:

- Normalized company names.
- Job titles.
- Locations.
- Additional attributes where available.

### Stage 4: Decision

Possible outcomes:

- Confirmed duplicate.
- Potential duplicate.
- New advertisement.

Confirmed duplicates shall be associated with the existing JobPosting.

Potential duplicates shall remain separate unless a sufficiently reliable rule confirms the match.

### Concurrency

Duplicate detection must also be supported by database constraints and transaction-safe persistence.

Application-level checks alone are insufficient to prevent race conditions.

## 11. Search Profile Architecture

Search profiles will be stored in SQL Server.

Each profile defines:

- Name.
- Keywords.
- Location.
- Remote preference.
- Enabled state.

The Blazor interface will provide profile management.

The API will expose profile operations.

The Application layer will validate profile changes.

Infrastructure will persist profiles.

## 12. Automatic Search Architecture

### Proposed Workflow

1. Background service starts.
2. Search schedule is loaded.
3. The system checks whether a search is due.
4. Enabled profiles are selected.
5. Provider usage limits are checked.
6. Available providers are called.
7. Results are normalized.
8. Duplicate detection is performed.
9. New advertisements are stored.
10. Existing advertisements receive additional source references when appropriate.
11. Search execution history is updated.
12. The next execution is scheduled.

### Startup Behavior

The system shall check for overdue searches when the background service starts.

The startup check must not cause uncontrolled repeated API requests.

### Manual Execution

A Search Now action will be exposed through the API.

Manual execution shall use the same application workflow as scheduled execution.

### Runtime Limitations

The background service operates only while its hosting process is running.

Future always-on deployment may use Docker and a continuously running host.

## 13. API Layer

The API provides REST endpoints for:

- Job advertisements.
- Source references.
- Search profiles.
- Search execution.
- Application tracking.
- Dashboard statistics.

Controllers should delegate business logic to Application services.

The API project acts as the primary dependency injection composition root for backend services.

## 14. Presentation Layer

The frontend uses Blazor Web App.

Planned pages:

- Dashboard.
- Job Listings.
- Job Details.
- Saved Jobs.
- Applications.
- Search Profiles.
- Search History.

Planned controls:

- Search Now button.
- Status selectors.
- Search filters.
- Profile creation and editing forms.
- External source links.

Presentation communicates with API through HTTP.

## 15. Error Handling

The application shall distinguish:

- Validation errors.
- Resource not found errors.
- Duplicate conflicts.
- Provider authentication failures.
- Provider rate limits.
- Provider availability failures.
- Database errors.
- Unexpected internal errors.

API responses should use a consistent error format.

## 16. Security

- Store API credentials securely.
- Validate input.
- Use HTTPS.
- Restrict access before public deployment.
- Validate external URLs.
- Avoid exposing sensitive data in logs.
- Respect provider terms and technical restrictions.

## 17. Testing Strategy

### Unit Tests

- Domain validation.
- Application services.
- Duplicate detection.
- Search profile rules.
- Application status transitions.

### Integration Tests

- SQL Server persistence.
- API endpoints.
- Dependency injection.
- Repository behavior.
- Database uniqueness constraints.

### Provider Tests

- Response normalization.
- HTTP error handling.
- Rate limiting.
- Empty results.
- Cancellation.

## 18. Architectural Decisions

### ADR-001: Clean Architecture

Chosen to isolate business logic and support testing.

### ADR-002: ASP.NET Core Web API

Chosen to provide a backend independent of the Blazor interface.

### ADR-003: Blazor Web App

Chosen to keep frontend development within the C# ecosystem.

### ADR-004: SQL Server

Chosen as the initial relational database.

### ADR-005: Multiple Sources per Job

A logical advertisement may have several source references.

### ADR-006: Conservative Duplicate Detection

Uncertain matches must not be automatically merged.

### ADR-007: BackgroundService Scheduling

Selected as the initial background execution mechanism.

### ADR-008: Authorized Provider Integration

Automated provider access must use permitted methods.

## 19. Future Improvements

- Email notifications.
- Advanced job matching.
- AI-assisted ranking.
- Containerized deployment.
- Authentication.
- Multi-user support.
- Distributed scheduling.
- Provider-specific request budgets.
- Manual duplicate review interface.
