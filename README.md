
# JobHunter

JobHunter is a personal job discovery and application tracking web application built with C# and .NET 10.

The application aims to collect job opportunities from supported sources, eliminate duplicate advertisements, and provide a centralized interface for managing search preferences and tracking applications.

## Project Status

**Status:** Planning and documentation completed for the initial MVP.

Implementation has not started yet.

## Project Objectives

- Discover junior-level IT job opportunities.
- Aggregate opportunities from multiple job platforms.
- Reduce repetitive manual job searching.
- Detect duplicate advertisements across sources.
- Provide configurable search profiles.
- Track job application statuses.
- Support automatic and manually triggered searches.
- Display search results and application statistics.
- Archive expired or outdated advertisements.

## Target Job Positions

Initial search categories:

- Junior C# / .NET Developer
- Junior Software Developer
- Junior Data Analyst
- Junior BI Analyst / Developer
- Junior SQL Developer
- Junior IT Support Specialist
- Junior System Administrator

Search categories and keywords will be configurable through the web interface.

## Target Locations

Initial preferences:

- Budapest, Hungary
- Budapest metropolitan area
- Remote positions available to applicants in Hungary

Supported work arrangements:

- On-site
- Hybrid
- Remote

## Job Sources

The application is designed to support:

### Jooble

Primary planned automated job source.

Integration through the official Jooble API, subject to API credentials, usage limits, and provider terms.

### LinkedIn

Target job discovery platform.

Initial support will focus on manually saving job advertisement links.

Automated integration will only be considered where officially authorized access is available.

### No Fluff Jobs

Target job discovery platform.

Initial support will focus on manually saving advertisement links.

Automated integration depends on identifying an authorized and technically supported access method.

## Main Features

### Job Advertisement Management

- Store job advertisements.
- View advertisement details.
- Save original source links.
- Filter advertisements.
- Archive advertisements.
- Update application status.

### Duplicate Detection

Duplicate detection will use:

1. Provider-specific advertisement identifiers.
2. Normalized advertisement URLs.
3. Company name similarity.
4. Job title similarity.
5. Location similarity.

Uncertain matches must not be automatically merged without sufficient confidence.

A single logical job posting may reference multiple external sources.

### Search Profiles

Users will be able to:

- Create search profiles.
- Edit search profiles.
- Enable or disable profiles.
- Delete search profiles.
- Configure keywords and preferred locations.
- Include or exclude remote opportunities.

### Automatic Search

The application will support scheduled background searches where an authorized provider integration is available.

Planned behavior:

- Check search schedules when the application starts.
- Execute overdue searches when appropriate.
- Run searches automatically while the application is running.
- Support manual execution through a Search Now button.
- Respect provider API usage limits.
- Prevent overlapping duplicate search executions.

The initial schedule will be configurable rather than permanently fixed.

### Application Tracking

Supported statuses:

- New
- Saved
- Applied
- Interview
- Rejected
- OfferReceived

Applications will initially be submitted manually on the original job platform.

### Dashboard

The dashboard will display:

- Total job advertisements.
- New opportunities.
- Saved opportunities.
- Submitted applications.
- Interviews.
- Offers received.
- Recent search execution results.

## Technology Stack

| Component | Technology |
|---|---|
| Language | C# |
| Runtime | .NET 10 |
| Backend | ASP.NET Core Web API |
| Frontend | Blazor Web App |
| Architecture | Clean Architecture |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Testing | xUnit |
| Version Control | Git |
| Repository Hosting | GitHub |

## Solution Projects

- JobHunter.Domain
- JobHunter.Application
- JobHunter.Infrastructure
- JobHunter.API
- JobHunter.Presentation
- JobHunter.Test

## Architecture

The application follows Clean Architecture principles.

- Domain contains business entities and rules.
- Application contains use cases and abstractions.
- Infrastructure implements persistence, job providers, and background processing.
- API exposes HTTP endpoints.
- Presentation provides the Blazor interface.
- Test contains automated tests.

The Presentation project communicates with the API through HTTP.

## Development Scope

### Initial MVP

- Job posting management.
- Editable search profiles.
- Duplicate detection.
- Application status tracking.
- SQL Server persistence.
- REST API.
- Blazor user interface.
- Jooble integration, subject to API access.
- Scheduled and manual search execution.
- Basic dashboard.
- Automated tests.

### Future Features

- Additional authorized provider integrations.
- Email notifications.
- Advanced job ranking.
- CV-to-job matching.
- Application assistance.
- Docker deployment.
- Multi-user support.

## Documentation

- [Requirements](docs/Requirements.md)
- [Architecture](docs/Architecture.md)
- [Database Design](docs/DatabaseDesign.md)
- [API Specification](docs/API.md)
- [Development Guide](docs/DevelopmentGuide.md)
- [Testing Strategy](docs/Testing.md)
- [Development Roadmap](docs/Roadmap.md)

## Development Environment

Planned environment:

- Visual Studio 2026
- .NET 10 SDK
- SQL Server or LocalDB
- Git

## Security and Compliance

- Store API keys outside source control.
- Use HTTPS for external API communication.
- Validate incoming data and external URLs.
- Respect external platform terms and API limits.
- Avoid unauthorized scraping or automated applications.
- Protect personal application data before exposing the application over a network.

## License

No software license has been selected yet.
