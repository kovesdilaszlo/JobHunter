
# JobHunter - Development Roadmap

## 1. Project Vision

JobHunter is a personal job discovery and application tracking platform for junior IT opportunities.

The system will aggregate job advertisements, eliminate confirmed duplicates, provide editable search preferences, and track applications through a web interface.

Development will follow incremental and testable milestones.

## 2. Current Project Status

**Phase:** Planning and documentation.

Completed activities:

- [x] Define project objectives.
- [x] Select technology stack.
- [x] Define Clean Architecture.
- [x] Create six Visual Studio projects.
- [x] Create project folder structure.
- [x] Configure project references.
- [x] Define initial functional requirements.
- [x] Define external job sources.
- [x] Define duplicate detection strategy.
- [x] Define automatic search behavior.
- [x] Prepare initial documentation.

Pending:

- [ ] Verify complete Solution build.
- [ ] Implement Domain layer.
- [ ] Implement Application layer.
- [ ] Implement Infrastructure layer.
- [ ] Implement API endpoints.
- [ ] Implement Blazor interface.
- [ ] Implement automated tests.

## 3. Phase 1 - Project Foundation

### Objective

Prepare a stable development environment.

### Tasks

- [ ] Verify Solution build.
- [ ] Verify project references.
- [ ] Configure Git repository.
- [ ] Configure GitHub repository.
- [ ] Review .gitignore.
- [ ] Verify .NET 10 compatibility.
- [ ] Review documentation consistency.

### Deliverable

A buildable and documented Solution.

## 4. Phase 2 - Domain Layer

### Objective

Implement the core business model.

### Tasks

- [ ] Implement JobPosting.
- [ ] Implement JobPostingSource.
- [ ] Implement JobApplication.
- [ ] Implement JobSearchProfile.
- [ ] Define ApplicationStatus.
- [ ] Define JobSourceType.
- [ ] Define WorkArrangement.
- [ ] Implement entity validation.
- [ ] Implement domain business rules.
- [ ] Write Domain unit tests.

### Deliverable

A tested Domain layer representing advertisements, source references, and search profiles.

## 5. Phase 3 - Application Layer

### Objective

Implement business workflows independently of technical infrastructure.

### Tasks

- [ ] Define repository interfaces.
- [ ] Define service interfaces.
- [ ] Define provider abstraction.
- [ ] Implement DTOs.
- [ ] Implement JobService.
- [ ] Implement SearchProfileService.
- [ ] Implement application status management.
- [ ] Implement duplicate detection service.
- [ ] Implement search coordination.
- [ ] Write Application unit tests.

### Deliverable

Testable application services and interfaces.

## 6. Phase 4 - Persistence

### Objective

Store application data in SQL Server.

### Tasks

- [ ] Install EF Core packages.
- [ ] Configure SQL Server.
- [ ] Implement JobHunterDbContext.
- [ ] Configure entity relationships.
- [ ] Configure database constraints.
- [ ] Implement repositories.
- [ ] Create initial migration.
- [ ] Apply migration.
- [ ] Test duplicate uniqueness constraints.
- [ ] Write persistence integration tests.

### Deliverable

Functional relational persistence.

## 7. Phase 5 - REST API

### Objective

Expose application functionality through HTTP.

### Tasks

- [ ] Configure dependency injection.
- [ ] Implement JobsController.
- [ ] Implement SearchProfilesController.
- [ ] Implement application status endpoints.
- [ ] Implement source reference handling.
- [ ] Implement validation.
- [ ] Implement consistent error responses.
- [ ] Configure OpenAPI.
- [ ] Write API integration tests.

### Deliverable

A functional REST API.

## 8. Phase 6 - Blazor Web Interface

### Objective

Provide a usable interface for job management.

### Tasks

- [ ] Configure API HttpClient.
- [ ] Create dashboard page.
- [ ] Create job listing page.
- [ ] Create job details page.
- [ ] Add search filters.
- [ ] Add manual job entry form.
- [ ] Add application status controls.
- [ ] Add source link display.
- [ ] Add advertisement archiving.
- [ ] Add loading and error states.
- [ ] Implement responsive layout.

### Deliverable

A functional Blazor interface connected to the API.

## 9. Phase 7 - Editable Search Profiles

### Objective

Allow search preferences to be managed through the web interface.

### Tasks

- [ ] Create search profile listing page.
- [ ] Implement profile creation.
- [ ] Implement profile editing.
- [ ] Implement profile deletion.
- [ ] Implement enable and disable controls.
- [ ] Support keyword configuration.
- [ ] Support location configuration.
- [ ] Support remote work preferences.
- [ ] Validate profile input.
- [ ] Write relevant tests.

### Deliverable

Fully editable search profiles.

## 10. Phase 8 - Jooble Integration

### Objective

Implement the first automated job provider.

### Tasks

- [ ] Obtain authorized Jooble API credentials.
- [ ] Review current API documentation.
- [ ] Confirm applicable request limits.
- [ ] Implement JoobleJobProvider.
- [ ] Implement request construction.
- [ ] Implement response normalization.
- [ ] Handle API errors.
- [ ] Handle rate limits.
- [ ] Store retrieved advertisements.
- [ ] Add source references.
- [ ] Test provider behavior with mocked HTTP responses.

### Deliverable

Automated job discovery through Jooble, subject to API access.

## 11. Phase 9 - LinkedIn and No Fluff Jobs

### Objective

Support advertisements from additional target platforms.

### Tasks

- [ ] Support manual LinkedIn advertisement links.
- [ ] Support manual No Fluff Jobs advertisement links.
- [ ] Identify source type from supported URLs where practical.
- [ ] Validate URLs.
- [ ] Associate source links with logical advertisements.
- [ ] Investigate authorized automated integration options.
- [ ] Document integration restrictions.

### Deliverable

Manual link management for LinkedIn and No Fluff Jobs.

Automated collection from these platforms is not required for the MVP.

## 12. Phase 10 - Duplicate Detection

### Objective

Prevent duplicate logical advertisements.

### Tasks

- [ ] Implement URL normalization.
- [ ] Implement provider identifier matching.
- [ ] Implement URL fingerprinting.
- [ ] Implement company name normalization.
- [ ] Implement job title normalization.
- [ ] Implement location comparison.
- [ ] Define conservative matching rules.
- [ ] Associate confirmed duplicate sources.
- [ ] Preserve uncertain matches separately.
- [ ] Test database concurrency behavior.
- [ ] Write duplicate detection tests.

### Deliverable

Reliable duplicate prevention and multi-source advertisement support.

## 13. Phase 11 - Automatic Search Scheduling

### Objective

Automate job searches while the backend host is running.

### Tasks

- [ ] Implement BackgroundService.
- [ ] Configure search intervals.
- [ ] Check overdue searches on startup.
- [ ] Execute enabled search profiles.
- [ ] Respect provider usage limits.
- [ ] Prevent overlapping executions.
- [ ] Implement cancellation.
- [ ] Record execution history.
- [ ] Handle provider failures.
- [ ] Add manual Search Now functionality.
- [ ] Write scheduling tests.

### Deliverable

Automatic and manually triggered job discovery.

## 14. Phase 12 - Dashboard

### Objective

Provide a centralized overview of the job search.

### Tasks

- [ ] Display total advertisements.
- [ ] Display newly discovered advertisements.
- [ ] Display saved advertisements.
- [ ] Display submitted applications.
- [ ] Display interviews.
- [ ] Display offers received.
- [ ] Display recent search executions.
- [ ] Add navigation to relevant records.

### Deliverable

Interactive job search dashboard.

## 15. Phase 13 - Quality Assurance

### Objective

Validate the complete application.

### Tasks

- [ ] Expand Domain tests.
- [ ] Expand Application tests.
- [ ] Expand persistence tests.
- [ ] Expand API tests.
- [ ] Test duplicate detection.
- [ ] Test provider failures.
- [ ] Test scheduled execution.
- [ ] Test Blazor workflows.
- [ ] Review logging.
- [ ] Review security.
- [ ] Review documentation.

### Deliverable

A tested MVP.

## 16. Phase 14 - Deployment Preparation

### Objective

Prepare the application for reliable local or self-hosted use.

### Tasks

- [ ] Configure environment-specific settings.
- [ ] Protect credentials.
- [ ] Document database setup.
- [ ] Document backup procedures.
- [ ] Configure structured logging.
- [ ] Investigate Docker deployment.
- [ ] Configure optional CI workflow.
- [ ] Document startup and shutdown behavior.

### Deliverable

A deployable and maintainable application.

## 17. MVP Definition

The MVP must support:

1. Manual job advertisement management.
2. Multiple source references per advertisement.
3. Editable search profiles.
4. Application status tracking.
5. Advertisement filtering.
6. Confirmed duplicate prevention.
7. SQL Server persistence.
8. REST API.
9. Blazor interface.
10. Jooble integration, subject to authorized access.
11. Manual LinkedIn link support.
12. Manual No Fluff Jobs link support.
13. Automatic and manual search execution.
14. Dashboard statistics.
15. Automated tests.

## 18. Future Development

Potential future features:

- Email notifications.
- CV-to-job matching.
- AI-assisted job ranking.
- Advanced search recommendations.
- Interview calendar integration.
- Application reminders.
- Additional authorized job providers.
- Docker-based continuous hosting.
- Multi-user accounts.
- Authentication and authorization.
- Manual duplicate review.
- Assisted application preparation.

Automatic application submission is not part of the MVP.

## 19. Development Workflow

Each milestone should follow this process:

1. Review relevant requirements.
2. Implement the smallest functional component.
3. Write or update tests.
4. Build the Solution.
5. Execute relevant tests.
6. Update documentation.
7. Commit changes to Git.
8. Proceed to the next milestone.

## 20. Definition of Done

A milestone is complete when:

- Its required functionality is implemented.
- The Solution builds successfully.
- Relevant automated tests pass.
- Critical defects are resolved.
- Documentation reflects actual behavior.
- Changes are committed to version control.
