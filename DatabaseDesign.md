
# JobHunter - Database Design

## 1. Overview

JobHunter uses a relational database to store job advertisements, application tracking information, search preferences, and background execution history.

Database engine: Microsoft SQL Server.

ORM: Entity Framework Core 10.

The database schema will be managed through EF Core migrations.

This document describes the proposed data model. It does not imply that all tables have already been implemented.

## 2. Entity Overview

The initial database design includes:

- JobPosting
- JobApplication
- JobSearchProfile
- SearchExecution
- SearchExecutionItem (optional future extension)

## 3. JobPosting

Stores individual job advertisements.

| Column | Type | Required | Description |
|---|---|---|---|
| Id | uniqueidentifier | Yes | Primary key |
| Title | nvarchar(250) | Yes | Job title |
| Company | nvarchar(200) | Yes | Company name |
| Location | nvarchar(250) | Yes | Job location |
| Description | nvarchar(max) | Yes | Job description |
| Url | nvarchar(2048) | Yes | Original URL |
| NormalizedUrl | nvarchar(2048) | Yes | Duplicate detection |
| Source | int | Yes | Job source enum |
| ExternalId | nvarchar(250) | No | Provider-specific identifier |
| IsRemote | bit | Yes | Remote work indicator |
| SalaryMin | decimal(18,2) | No | Minimum salary |
| SalaryMax | decimal(18,2) | No | Maximum salary |
| Currency | nvarchar(3) | No | ISO currency code |
| PublishedAt | datetimeoffset | No | Publication timestamp |
| DiscoveredAt | datetimeoffset | Yes | Discovery timestamp |
| Status | int | Yes | Application status |

### Constraints

- Id is the primary key.
- Required string fields must not be empty.
- Salary values must not be negative.
- SalaryMin must not exceed SalaryMax when both are provided.
- URLs must use supported HTTP or HTTPS schemes.
- Duplicate detection must be enforced consistently.

### Indexes

Recommended indexes:

- Unique index on NormalizedUrl.
- Index on DiscoveredAt.
- Index on Status.
- Index on Source.
- Index on Company.

URL normalization rules must be defined before enabling a unique index in production.

## 4. JobApplication

Stores application-specific information.

| Column | Type | Required | Description |
|---|---|---|---|
| Id | uniqueidentifier | Yes | Primary key |
| JobPostingId | uniqueidentifier | Yes | Related job |
| AppliedAt | datetimeoffset | No | Submission timestamp |
| Status | int | Yes | Application status |
| Notes | nvarchar(max) | No | Personal notes |
| LastUpdatedAt | datetimeoffset | Yes | Last update |

### Relationships

JobPosting has zero or one JobApplication in the initial single-user design.

A unique constraint on JobPostingId enforces this relationship.

In future multi-user versions, the relationship may be redesigned to support multiple applications for the same posting.

## 5. JobSearchProfile

Stores reusable search preferences.

| Column | Type | Required | Description |
|---|---|---|---|
| Id | uniqueidentifier | Yes | Primary key |
| Name | nvarchar(150) | Yes | Profile name |
| Keywords | nvarchar(max) | Yes | Search terms |
| Location | nvarchar(250) | No | Preferred location |
| IncludeRemote | bit | Yes | Include remote jobs |
| IsEnabled | bit | Yes | Profile active |
| CreatedAt | datetimeoffset | Yes | Creation timestamp |
| UpdatedAt | datetimeoffset | Yes | Last modification |

### Initial Search Profiles

Suggested profiles:

1. Junior .NET Developer.
2. Junior Software Developer.
3. Junior Data Analyst.
4. Junior BI Analyst.
5. Junior SQL Developer.
6. Junior IT Support.
7. Junior System Administrator.

Search profiles may later support structured keyword collections instead of a single text field.

## 6. SearchExecution

Stores background search execution history.

| Column | Type | Required | Description |
|---|---|---|---|
| Id | uniqueidentifier | Yes | Primary key |
| ProviderName | nvarchar(150) | Yes | Provider identifier |
| StartedAt | datetimeoffset | Yes | Start time |
| CompletedAt | datetimeoffset | No | Completion time |
| Status | int | Yes | Execution result |
| RetrievedCount | int | Yes | Retrieved advertisements |
| InsertedCount | int | Yes | New advertisements |
| ErrorMessage | nvarchar(max) | No | Sanitized error details |

### Execution Status Values

Suggested statuses:

- Pending.
- Running.
- Completed.
- Failed.
- PartiallyCompleted.

## 7. Relationships

```text
JobPosting
    |
    | 1 : 0..1
    |
JobApplication

JobSearchProfile
    |
    | 1 : N (future)
    |
SearchExecution
```

A future SearchExecution model may reference JobSearchProfile directly.

## 8. Entity Framework Core

The DbContext will be implemented in the Infrastructure project.

Proposed location:

```text
JobHunter.Infrastructure/
└── Persistence/
    ├── JobHunterDbContext.cs
    ├── Configurations/
    └── Migrations/
```

Example DbSet declarations:

```csharp
public DbSet<JobPosting> JobPostings => Set<JobPosting>();

public DbSet<JobApplication> JobApplications => Set<JobApplication>();

public DbSet<JobSearchProfile> JobSearchProfiles => Set<JobSearchProfile>();
```

These declarations require the corresponding entity classes.

## 9. Migration Strategy

Database schema changes shall be implemented through EF Core migrations.

Recommended process:

1. Modify entity definitions.
2. Update EF Core configurations.
3. Create a migration.
4. Review generated migration code.
5. Apply the migration to the development database.
6. Run relevant tests.

Production migrations require backup and rollback planning.

## 10. Data Integrity

The database should enforce:

- Primary keys.
- Foreign keys.
- Unique constraints.
- Required fields.
- Valid numeric ranges where practical.
- Referential integrity.

Business validation must also be performed in the Domain and Application layers.

## 11. Data Retention

The system shall allow old advertisements to be archived or removed.

Future retention policies should cover:

- Expired advertisements.
- Search execution logs.
- Application history.
- External provider response data.

## 12. Security and Privacy

- Store only necessary personal information.
- Avoid storing external account passwords.
- Keep database credentials outside source control.
- Restrict database access.
- Avoid logging sensitive application notes.
- Use parameterized queries through EF Core.
