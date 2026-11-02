
# JobHunter - Development Roadmap

## 1. Project Vision

JobHunter aims to provide a centralized platform for discovering, reviewing, organizing, and tracking junior IT job opportunities.

Development will follow incremental, testable milestones.

## 2. Development Principles

- Implement core functionality before advanced automation.
- Keep the solution buildable.
- Add automated tests with new functionality.
- Maintain Clean Architecture boundaries.
- Prefer supported external APIs.
- Avoid unnecessary complexity.
- Document significant decisions.

## 3. Phase 1 - Solution Foundation

### Objectives

Establish the development environment and project architecture.

### Tasks

- [x] Define the six-project architecture.
- [x] Create the initial Solution structure.
- [x] Create project directories.
- [x] Configure project dependencies.
- [ ] Verify a successful complete Solution build.
- [ ] Configure Git and GitHub.
- [ ] Add initial documentation.

### Deliverable

A buildable .NET 10 Solution with documented architecture.

## 4. Phase 2 - Domain Model

### Objectives

Implement the core business entities and validation rules.

### Tasks

- [ ] Implement JobPosting.
- [ ] Implement ApplicationStatus.
- [ ] Implement JobSourceType.
- [ ] Implement JobApplication.
- [ ] Implement JobSearchProfile.
- [ ] Add domain validation.
- [ ] Write domain unit tests.

### Deliverable

A tested Domain layer.

## 5. Phase 3 - Application Layer

### Objectives

Implement business workflows independent of infrastructure.

### Tasks

- [ ] Define repository interfaces.
- [ ] Define service interfaces.
- [ ] Create request and response DTOs.
- [ ] Implement JobService.
- [ ] Implement status management.
- [ ] Implement filtering logic.
- [ ] Implement duplicate detection rules.
- [ ] Write application unit tests.

### Deliverable

Testable application services.

## 6. Phase 4 - Database and Persistence

### Objectives

Persist job advertisements and application data.

### Tasks

- [ ] Configure Entity Framework Core.
- [ ] Configure SQL Server connection.
- [ ] Implement JobHunterDbContext.
- [ ] Configure entity mappings.
- [ ] Implement repositories.
- [ ] Create initial database migration.
- [ ] Apply database migration.
- [ ] Write persistence integration tests.

### Deliverable

Working SQL Server persistence.

## 7. Phase 5 - REST API

### Objectives

Expose application functionality through HTTP.

### Tasks

- [ ] Configure dependency injection.
- [ ] Implement JobsController.
- [ ] Add job creation endpoint.
- [ ] Add job retrieval endpoints.
- [ ] Add application status endpoint.
- [ ] Add validation and error handling.
- [ ] Configure OpenAPI documentation.
- [ ] Write API integration tests.

### Deliverable

A functional REST API.

## 8. Phase 6 - Blazor User Interface

### Objectives

Provide a usable web interface.

### Tasks

- [ ] Configure API HttpClient.
- [ ] Create job listing page.
- [ ] Create job detail view.
- [ ] Add filtering controls.
- [ ] Add application status controls.
- [ ] Add saved jobs view.
- [ ] Add loading and error states.
- [ ] Implement responsive layout.

### Deliverable

A working web-based job management interface.

## 9. Phase 7 - External Job Providers

### Objectives

Collect job advertisements from supported sources.

### Tasks

- [ ] Define IJobProvider interface.
- [ ] Define external job DTOs.
- [ ] Implement provider normalization.
- [ ] Integrate one authorized provider.
- [ ] Handle API failures.
- [ ] Handle rate limits.
- [ ] Implement duplicate detection.
- [ ] Record source metadata.
- [ ] Add provider tests.

### Deliverable

Automated collection from at least one provider.

## 10. Phase 8 - Scheduled Job Collection

### Objectives

Automate searches twice per day.

### Tasks

- [ ] Implement background processing.
- [ ] Configure search schedule.
- [ ] Support multiple search profiles.
- [ ] Record execution history.
- [ ] Prevent overlapping executions.
- [ ] Handle application restarts.
- [ ] Add retry and failure handling.
- [ ] Write scheduling tests.

### Deliverable

Reliable scheduled job discovery.

## 11. Phase 9 - Dashboard and Analytics

### Objectives

Provide useful job search statistics.

### Tasks

- [ ] Count discovered advertisements.
- [ ] Count saved opportunities.
- [ ] Count submitted applications.
- [ ] Track interview stages.
- [ ] Track offers received.
- [ ] Display search history.
- [ ] Add dashboard components.

### Deliverable

An interactive job search dashboard.

## 12. Phase 10 - Quality and Deployment

### Objectives

Prepare the application for reliable long-term use.

### Tasks

- [ ] Review security configuration.
- [ ] Add structured logging.
- [ ] Improve exception handling.
- [ ] Expand automated tests.
- [ ] Configure GitHub Actions.
- [ ] Add Docker support.
- [ ] Document deployment.
- [ ] Verify backup and recovery procedures.

### Deliverable

A tested, documented, deployable application.

## 13. Future Features

Potential improvements:

- AI-assisted job ranking.
- CV-to-job matching.
- Personalized job recommendations.
- Email notifications.
- Additional provider integrations.
- Application reminders.
- Interview calendar integration.
- Export to CSV or Excel.
- Multi-user accounts.
- Role-based access control.
- Mobile-friendly progressive web app.
- Assisted application form completion.

Any automated application submission must require appropriate user authorization and comply with platform restrictions.

## 14. MVP Definition

The minimum viable product includes:

1. Job advertisement storage.
2. Manual job creation.
3. Job listing and details.
4. Basic filtering.
5. Duplicate prevention.
6. Application status management.
7. Blazor interface.
8. At least one external job provider.
9. Twice-daily background collection.
10. Automated tests.

## 15. Progress Tracking

Each phase should be completed with:

- Working code.
- Passing tests.
- Updated documentation.
- A Git commit.
- A clearly identified next milestone.

Development should proceed in small increments rather than implementing all features simultaneously.
