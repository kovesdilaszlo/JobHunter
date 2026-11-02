
# JobHunter - Software Requirements Specification

## 1. Introduction

### 1.1 Purpose

This document defines the functional and non-functional requirements of the JobHunter application.

JobHunter is intended to simplify the discovery, organization, and tracking of employment opportunities in the IT sector.

### 1.2 Scope

The application provides a centralized system for collecting and reviewing job advertisements from supported sources.

The initial release focuses on:

- Job advertisement storage.
- Manual job entry.
- Search and filtering.
- Duplicate prevention.
- Application status management.
- Basic external source integration.
- Scheduled job discovery.

Automated submission of job applications is outside the initial release scope.

### 1.3 Target User

The initial system is designed for a single user seeking junior-level IT positions in Hungary.

The architecture should allow future support for multiple users.

## 2. Functional Requirements

### FR-001: Job Advertisement Management

The system shall allow the user to:

- View stored job advertisements.
- View detailed job information.
- Manually add job advertisements.
- Update application status.
- Remove or archive advertisements.

Each advertisement shall have a unique identifier.

### FR-002: Job Information

Each advertisement shall contain:

- Job title.
- Company name.
- Location.
- Description.
- Original advertisement URL.
- Source.
- Discovery timestamp.
- Application status.

Optional fields:

- Salary range.
- Publication date.
- Remote work indicator.
- Employment type.
- Required technologies.

### FR-003: External Job Sources

The system shall support integration with external job advertisement providers.

Potential providers include:

- Jooble
- Adzuna
- Greenhouse
- Other providers offering permitted API access

LinkedIn, Profession, and Indeed may be supported through authorized integrations or manual links where automated access is restricted.

Provider integrations shall be independently configurable.

### FR-004: Duplicate Detection

The system shall prevent duplicate advertisements from being stored.

The initial duplicate detection strategy shall use normalized source URLs.

Future strategies may include:

- Provider-specific advertisement identifiers.
- Company and title matching.
- Content similarity analysis.

### FR-005: Job Search Filters

The user shall be able to filter advertisements by:

- Job title.
- Company.
- Location.
- Remote work availability.
- Source.
- Application status.
- Publication date.
- Salary range, when available.

### FR-006: Search Preferences

The system shall support configurable search preferences.

Initial preferred positions:

- Junior C# / .NET Developer
- Junior Software Developer
- Junior Data Analyst
- Junior BI Analyst / Developer
- Junior SQL Developer
- Junior IT Support Specialist
- Junior System Administrator

Initial preferred location:

- Budapest and surrounding areas.

Remote and hybrid opportunities shall also be considered.

### FR-007: Application Tracking

Each job advertisement shall support the following statuses:

| Status | Description |
|---|---|
| New | Newly discovered opportunity |
| Saved | Selected for later review |
| Applied | Application submitted |
| Interview | Interview stage reached |
| Rejected | Application rejected |
| OfferReceived | Job offer received |

Future versions may introduce additional statuses and a detailed application history.

### FR-008: Scheduled Searches

The system shall support scheduled collection of advertisements.

Initial schedule:

- Twice daily.
- Configurable execution times.
- Execution independent of user interaction.

The system shall record execution results and failures.

### FR-009: Dashboard

The dashboard shall display:

- Total stored advertisements.
- Newly discovered opportunities.
- Saved advertisements.
- Submitted applications.
- Interviews.
- Offers received.

### FR-010: External Links

The user shall be able to open the original job advertisement in a browser.

External URLs shall be validated before storage and presentation.

### FR-011: Search Execution History

The system shall record:

- Search execution time.
- Provider name.
- Execution status.
- Number of retrieved advertisements.
- Number of newly stored advertisements.
- Error details, where applicable.

## 3. Non-Functional Requirements

### NFR-001: Maintainability

The application shall follow Clean Architecture principles and separate domain logic from infrastructure concerns.

### NFR-002: Reliability

Failures in one external provider shall not prevent other providers from being processed.

### NFR-003: Performance

Database queries shall use appropriate indexing and asynchronous operations.

Performance targets shall be established and validated using representative workloads.

### NFR-004: Security

- Validate user input.
- Use HTTPS for external communication.
- Store credentials securely.
- Do not expose secrets in source control.
- Protect sensitive operations from unauthorized access.

### NFR-005: Testability

Core business rules and application services shall support automated unit testing.

Database and API behavior shall support integration testing.

### NFR-006: Extensibility

Additional job providers shall be integrated without modifying core domain entities unnecessarily.

### NFR-007: Usability

The web interface shall provide clear navigation, readable job listings, and responsive layouts.

### NFR-008: Observability

Application errors and scheduled job execution results shall be logged.

## 4. Constraints

- Development platform: .NET 10.
- Primary database: SQL Server.
- Frontend framework: Blazor.
- Initial deployment: local development environment.
- External API usage depends on provider availability and authorization.
- Automated applications require separate design and approval.

## 5. Acceptance Criteria

The initial MVP shall be considered complete when:

1. The application builds successfully.
2. The database schema can be created through EF Core migrations.
3. Job advertisements can be stored and retrieved.
4. Duplicate URLs are handled correctly.
5. Application statuses can be updated.
6. Advertisements can be filtered.
7. The Blazor interface communicates with the API.
8. At least one authorized external provider is integrated.
9. Scheduled collection can execute twice daily.
10. Automated tests pass.

## 6. Out of Scope for MVP

- Automatic CV submission.
- Automatic login to job platforms.
- AI-generated cover letters.
- Multi-user account management.
- Advanced machine-learning ranking.
- Mobile native applications.
