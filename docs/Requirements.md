
# JobHunter - Software Requirements Specification

## 1. Introduction

### 1.1 Purpose

This document defines the functional and non-functional requirements of JobHunter.

JobHunter is a personal job discovery, aggregation, and application tracking system focused on junior IT employment opportunities.

### 1.2 Scope

The application will provide:

- Job advertisement management.
- Search profile management.
- Integration with supported job platforms.
- Duplicate advertisement detection.
- Automatic and manual job searches.
- Application status tracking.
- Dashboard statistics.
- Advertisement archiving.

The initial version is intended for a single user.

### 1.3 Development Status

The project is currently in the planning and documentation phase.

The requirements describe intended functionality, not completed implementation.

## 2. Target Job Categories

Initial target positions:

- Junior C# / .NET Developer
- Junior Software Developer
- Junior Data Analyst
- Junior BI Analyst / Developer
- Junior SQL Developer
- Junior IT Support Specialist
- Junior System Administrator

The user must be able to modify search keywords through the web interface.

## 3. Target Locations

Initial location preferences:

- Budapest
- Budapest metropolitan area
- Remote opportunities available in Hungary

Supported work arrangements:

- On-site
- Hybrid
- Remote

Location preferences must be configurable.

## 4. External Job Sources

### FR-001: Jooble Integration

The system shall support integration with the official Jooble API.

The integration shall:

- Authenticate using configured API credentials.
- Submit searches based on enabled profiles.
- Normalize returned advertisements.
- Handle API errors.
- Respect usage limits.
- Record search execution results.

Availability depends on provider authorization and applicable API restrictions.

### FR-002: LinkedIn Support

The system shall support manual storage of LinkedIn job advertisement links.

Automated collection shall not be required for the MVP.

Future automated integration may be introduced only through authorized methods.

### FR-003: No Fluff Jobs Support

The system shall support manual storage of No Fluff Jobs advertisement links.

Automated collection shall depend on the availability of an authorized integration method.

### FR-004: Extensible Providers

The system shall support adding new providers without significant changes to the Domain layer.

Each provider shall have an independent implementation.

## 5. Job Advertisement Management

### FR-005: Store Job Advertisements

The system shall store:

- Unique identifier.
- Job title.
- Company name.
- Location.
- Description.
- Work arrangement.
- Discovery timestamp.
- Publication timestamp, if available.
- Salary information, if available.
- Application status.
- Associated source links.

### FR-006: View Advertisements

The user shall be able to:

- View all stored advertisements.
- Open advertisement details.
- View source links.
- Open the original advertisement.
- View discovery and publication dates.

### FR-007: Manual Advertisement Creation

The user shall be able to manually add an advertisement using a source URL and relevant job information.

The system shall validate required fields.

### FR-008: Advertisement Filtering

Supported filters shall include:

- Job title.
- Company.
- Location.
- Work arrangement.
- Source.
- Application status.
- Discovery date.
- Publication date.
- Salary range, when available.

### FR-009: Advertisement Archiving

The user shall be able to archive advertisements.

Archived advertisements shall remain stored but may be excluded from default search results.

## 6. Duplicate Detection

### FR-010: Duplicate Identification

The system shall identify potential duplicate advertisements using multiple signals.

Primary signals:

1. Provider identifier and external advertisement identifier.
2. Normalized source URL.

Secondary signals:

1. Normalized company name.
2. Similarity of job titles.
3. Location similarity.
4. Additional advertisement attributes where available.

### FR-011: Duplicate Handling

Confirmed duplicates shall not create additional logical job postings.

Instead, new source references shall be associated with the existing posting.

### FR-012: Uncertain Matches

Advertisements with uncertain similarity shall not be automatically merged.

The system may mark these advertisements for review in a future version.

### FR-013: Multiple Source Links

A logical job posting shall support multiple source references.

Each reference shall identify:

- Source provider.
- Original URL.
- Normalized URL.
- External advertisement identifier, if available.
- Discovery timestamp.

## 7. Search Profile Management

### FR-014: Create Search Profiles

The user shall be able to create search profiles through the web interface.

A profile shall include:

- Profile name.
- Search keywords.
- Preferred location.
- Remote work preference.
- Enabled or disabled state.

### FR-015: Edit Search Profiles

The user shall be able to modify existing profiles.

### FR-016: Delete Search Profiles

The user shall be able to remove search profiles.

### FR-017: Enable or Disable Profiles

Disabled profiles shall not be included in automatic searches.

### FR-018: Multiple Search Profiles

The system shall support multiple independently configured profiles.

## 8. Search Execution

### FR-019: Automatic Searches

The system shall support automatic search execution while the background processing host is running.

The execution schedule shall be configurable.

### FR-020: Startup Search Check

When the application starts, the background service shall check whether a search is overdue.

An overdue search may execute automatically if:

- The profile is enabled.
- Provider limits permit execution.
- Another equivalent search is not already running.
- Required credentials and configuration are available.

### FR-021: Manual Search

The user shall be able to trigger a search through the web interface.

A Search Now action shall be provided.

### FR-022: Provider Usage Limits

The system shall respect provider-specific usage limits.

It shall avoid unnecessary requests and prevent uncontrolled repeated searches.

### FR-023: Search Execution History

Each search execution shall record:

- Execution identifier.
- Provider.
- Search profile.
- Start time.
- Completion time.
- Execution status.
- Retrieved advertisement count.
- Newly inserted advertisement count.
- Error information, where applicable.

### FR-024: Failure Isolation

Failure of one provider shall not prevent other enabled providers from executing.

### FR-025: Execution Concurrency

The system shall prevent overlapping equivalent search executions.

## 9. Application Tracking

### FR-026: Application Status

Each logical advertisement shall support an application status.

Initial statuses:

- New
- Saved
- Applied
- Interview
- Rejected
- OfferReceived

### FR-027: Status Modification

The user shall be able to change application status through the web interface.

### FR-028: Application Notes

The system should support optional notes associated with an application.

### FR-029: Manual Application Submission

Applications shall initially be submitted manually on external job platforms.

Automatic application submission is outside the MVP scope.

## 10. Dashboard

### FR-030: Summary Statistics

The dashboard shall display:

- Total advertisements.
- New advertisements.
- Saved advertisements.
- Submitted applications.
- Interviews.
- Offers received.

### FR-031: Search Information

The dashboard shall display recent search execution results.

### FR-032: New Opportunities

Recently discovered advertisements shall be accessible from the dashboard.

## 11. Notifications

### FR-033: Initial Notifications

The MVP shall present new opportunities and relevant information through the dashboard.

### FR-034: Future Notifications

Email notifications may be added in a later release.

## 12. Non-Functional Requirements

### NFR-001: Architecture

The application shall follow Clean Architecture principles.

### NFR-002: Maintainability

Business logic shall remain independent of external provider implementations.

### NFR-003: Reliability

Provider failures shall be handled and logged.

### NFR-004: Performance

Database queries shall support pagination and appropriate indexing.

### NFR-005: Security

The system shall:

- Validate user input.
- Use HTTPS where applicable.
- Protect API credentials.
- Avoid committing secrets.
- Restrict access before network or public deployment.

### NFR-006: Testability

Domain rules, application services, and integrations shall support automated testing.

### NFR-007: Usability

The web interface shall be responsive and provide clear filtering and navigation.

### NFR-008: Extensibility

New providers, filters, and application statuses should be introducible with limited architectural changes.

### NFR-009: Observability

The application shall log errors and background execution results.

### NFR-010: Data Integrity

Duplicate prevention and relational constraints shall protect stored data.

## 13. Technical Constraints

- C# and .NET 10.
- ASP.NET Core Web API.
- Blazor Web App.
- Entity Framework Core.
- SQL Server.
- xUnit.
- Initial local deployment.
- External integrations subject to authorization and provider terms.

## 14. MVP Acceptance Criteria

The MVP is complete when:

1. The Solution builds successfully.
2. Core Domain entities are implemented.
3. SQL Server persistence is operational.
4. Advertisements can be added manually.
5. Advertisements can be viewed and filtered.
6. Search profiles can be managed through Blazor.
7. Application statuses can be modified.
8. Confirmed duplicate advertisements are prevented.
9. Multiple source references can be associated with one posting.
10. Jooble integration works with authorized credentials.
11. Automatic searches can execute while the application is running.
12. Startup search checks are operational.
13. Manual searches can be triggered.
14. Provider failures are handled.
15. Dashboard statistics are available.
16. Relevant automated tests pass.

## 15. Out of Scope

The following features are not required for the MVP:

- Automatic job application submission.
- Automated LinkedIn scraping.
- Automated No Fluff Jobs scraping.
- AI-generated cover letters.
- CV-to-job matching.
- Multi-user account management.
- Email notifications.
- Cloud deployment.
- Machine-learning-based recommendations.

## 16. Open Implementation Decisions

The following details may be finalized during implementation:

- Exact search schedule and time zone handling.
- Provider-specific query budgets.
- Duplicate similarity thresholds.
- Manual review of uncertain duplicates.
- Exact user interface layout.
- Background service deployment strategy.
