
# JobHunter

JobHunter is a personal job search aggregation and application tracking platform built with C# and .NET 10.

The application collects job advertisements from supported sources, filters opportunities based on user-defined preferences, and provides a centralized interface for reviewing and tracking job applications.

## Project Status

**Status:** Initial development

The solution structure and architectural design have been defined. Core business logic, persistence, external integrations, and user interface functionality are under development.

## Objectives

- Aggregate job opportunities from multiple sources.
- Reduce repetitive manual job searching.
- Detect duplicate job advertisements.
- Filter opportunities by role, location, and work arrangement.
- Track application progress.
- Schedule automatic searches twice per day.
- Provide a clean, responsive web interface.
- Support future automation of selected application workflows.

## Target Positions

- Junior C# / .NET Developer
- Junior Software Developer
- Junior Data Analyst
- Junior BI Analyst / Developer
- Junior SQL Developer
- Junior IT Support Specialist
- Junior System Administrator

## Preferred Locations

- Budapest, Hungary
- Budapest metropolitan area
- Remote opportunities available to candidates in Hungary

Supported work arrangements:

- On-site
- Hybrid
- Remote

## Technology Stack

| Component | Technology |
|---|---|
| Programming Language | C# |
| Runtime | .NET 10 |
| Backend | ASP.NET Core Web API |
| Frontend | Blazor Web App |
| Architecture | Clean Architecture |
| ORM | Entity Framework Core 10 |
| Database | SQL Server |
| Testing | xUnit |
| Version Control | Git |
| Repository Hosting | GitHub |

## Solution Structure

```text
JobHunter/
├── JobHunter.slnx
├── README.md
├── docs/
├── src/
│   ├── JobHunter.Domain/
│   ├── JobHunter.Application/
│   ├── JobHunter.Infrastructure/
│   ├── JobHunter.API/
│   └── JobHunter.Presentation/
└── tests/
    └── JobHunter.Test/
```

Actual project locations may differ depending on the Visual Studio solution layout.

## Architecture

The solution follows Clean Architecture principles.

- **Domain:** Business entities, enumerations, and rules.
- **Application:** Use cases, DTOs, service interfaces, and application services.
- **Infrastructure:** Database access, external integrations, and background processing.
- **API:** REST endpoints and application configuration.
- **Presentation:** Blazor-based user interface.
- **Test:** Automated unit and integration tests.

## Planned Features

1. Manual job posting management.
2. External job source integrations.
3. Duplicate detection.
4. Search filters and saved preferences.
5. Job application status tracking.
6. Scheduled background searches.
7. Dashboard and search history.
8. Optional application assistance.

## Documentation

- [Requirements](docs/Requirements.md)
- [Architecture](docs/Architecture.md)
- [Database Design](docs/DatabaseDesign.md)
- [API Specification](docs/API.md)
- [Development Guide](docs/DevelopmentGuide.md)
- [Testing Strategy](docs/Testing.md)
- [Roadmap](docs/Roadmap.md)

## Getting Started

### Prerequisites

- Visual Studio 2026
- .NET 10 SDK
- SQL Server or SQL Server LocalDB
- Git

### Build

```powershell
dotnet restore
dotnet build
```

### Run

Select `JobHunter.API` or `JobHunter.Presentation` as the startup project in Visual Studio.

The API, database, and frontend integration must be configured before all application features can be used.

## Security

Credentials, API keys, connection secrets, and other sensitive configuration values must not be committed to source control.

Automated interactions with external job platforms must respect applicable terms, privacy requirements, and technical restrictions.

## License

No license has been selected yet. All rights are reserved unless a license is added.
