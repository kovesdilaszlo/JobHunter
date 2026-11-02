
# JobHunter - Database Design

## 1. Overview

JobHunter will use Microsoft SQL Server with Entity Framework Core.

The database stores logical job advertisements, source references, application tracking information, search profiles, and search execution history.

This document defines the proposed database schema.

No database tables have been implemented yet.

## 2. Design Principles

- Use GUID primary keys.
- Maintain referential integrity.
- Separate logical advertisements from external source references.
- Support duplicate detection.
- Preserve application tracking information.
- Support configurable search profiles.
- Record background execution results.
- Use UTC-based timestamps.
- Manage schema changes through EF Core migrations.

## 3. Entity Overview

Planned entities:

1. JobPosting.
2. JobPostingSource.
3. JobApplication.
4. JobSearchProfile.
5. SearchExecution.

## 4. JobPosting

Represents one logical employment opportunity.

| Column | SQL Type | Required | Description |
|---|---|---|---|
| Id | uniqueidentifier | Yes | Primary key |
| Title | nvarchar(250) | Yes | Position title |
| Company | nvarchar(200) | Yes | Company name |
| Location | nvarchar(250) | Yes | Job location |
| Description | nvarchar(max) | Yes | Job description |
| WorkArrangement | int | Yes | On-site, hybrid, or remote |
| SalaryMin | decimal(18,2) | No | Minimum salary |
| SalaryMax | decimal(18,2) | No | Maximum salary |
| Currency | nvarchar(3) | No | Currency code |
| PublishedAt | datetimeoffset | No | Publication timestamp |
| DiscoveredAt | datetimeoffset | Yes | First discovery |
| Status | int | Yes | Application status |
| IsArchived | bit | Yes | Archive flag |
| ArchivedAt | datetimeoffset | No | Archive timestamp |

### Constraints

- Id is the primary key.
- Title and Company must not be empty.
- Salary values must not be negative.
- SalaryMin must not exceed SalaryMax.
- DiscoveredAt is required.
- ArchivedAt should be set when an advertisement is archived.

### Indexes

Recommended indexes:

- Status.
- DiscoveredAt.
- Company.
- IsArchived.
- WorkArrangement.

## 5. JobPostingSource

Represents one external reference to a logical advertisement.

A JobPosting may have multiple JobPostingSource records.

| Column | SQL Type | Required | Description |
|---|---|---|---|
| Id | uniqueidentifier | Yes | Primary key |
| JobPostingId | uniqueidentifier | Yes | Related logical job |
| SourceType | int | Yes | Provider identifier |
| ExternalId | nvarchar(250) | No | Provider advertisement ID |
| OriginalUrl | nvarchar(2048) | Yes | Source URL |
| NormalizedUrl | nvarchar(2048) | Yes | Normalized source URL |
| UrlFingerprint | varbinary(32) | Yes | SHA-256 fingerprint |
| DiscoveredAt | datetimeoffset | Yes | Discovery timestamp |
| LastSeenAt | datetimeoffset | No | Last provider observation |

### Relationships

JobPosting has a one-to-many relationship with JobPostingSource.

Every JobPostingSource must reference an existing JobPosting.

### Duplicate Constraints

Recommended uniqueness rules:

- Unique combination of SourceType and ExternalId when ExternalId is present.
- Unique UrlFingerprint for equivalent normalized URLs.

The full normalized URL must be retained to verify fingerprint matches.

A fixed-length fingerprint avoids SQL Server index key length limitations associated with long URL strings.

### Notes

URL normalization must preserve meaningful identifiers.

Tracking parameters may be removed only when known to be nonessential.

## 6. JobApplication

Stores application-specific information.

| Column | SQL Type | Required | Description |
|---|---|---|---|
| Id | uniqueidentifier | Yes | Primary key |
| JobPostingId | uniqueidentifier | Yes | Related job |
| AppliedAt | datetimeoffset | No | Submission timestamp |
| Status | int | Yes | Application status |
| Notes | nvarchar(max) | No | Personal notes |
| LastUpdatedAt | datetimeoffset | Yes | Last update |

### Relationship

In the initial single-user design:

JobPosting has zero or one JobApplication.

JobApplication has exactly one JobPosting.

A unique index on JobPostingId enforces this relationship.

### Status Consistency

JobPosting.Status is used for quick listing and filtering.

JobApplication.Status may represent detailed application tracking.

If both fields are retained, the application must define and enforce synchronization rules.

A future refinement may move status ownership to one entity to avoid redundant state.

## 7. JobSearchProfile

Stores editable job search preferences.

| Column | SQL Type | Required | Description |
|---|---|---|---|
| Id | uniqueidentifier | Yes | Primary key |
| Name | nvarchar(150) | Yes | Profile name |
| Keywords | nvarchar(max) | Yes | Search keywords |
| Location | nvarchar(250) | No | Preferred location |
| IncludeRemote | bit | Yes | Include remote jobs |
| IsEnabled | bit | Yes | Active profile |
| CreatedAt | datetimeoffset | Yes | Creation timestamp |
| UpdatedAt | datetimeoffset | Yes | Modification timestamp |

### Operations

The system shall support:

- Create.
- Read.
- Update.
- Delete.
- Enable.
- Disable.

### Initial Profiles

Suggested profiles:

- Junior .NET Developer.
- Junior Software Developer.
- Junior Data Analyst.
- Junior BI Analyst.
- Junior SQL Developer.
- Junior IT Support.
- Junior System Administrator.

Profiles are editable and not hardcoded permanently.

## 8. SearchExecution

Records automated and manual search executions.

| Column | SQL Type | Required | Description |
|---|---|---|---|
| Id | uniqueidentifier | Yes | Primary key |
| JobSearchProfileId | uniqueidentifier | Yes | Search profile |
| ProviderName | nvarchar(150) | Yes | Provider |
| StartedAt | datetimeoffset | Yes | Start timestamp |
| CompletedAt | datetimeoffset | No | Completion timestamp |
| Status | int | Yes | Execution status |
| TriggerType | int | Yes | Manual, startup, scheduled |
| RetrievedCount | int | Yes | Retrieved advertisements |
| InsertedCount | int | Yes | Newly stored advertisements |
| DuplicateCount | int | Yes | Detected duplicates |
| ErrorMessage | nvarchar(max) | No | Sanitized error details |

### Execution Statuses

- Pending.
- Running.
- Completed.
- Failed.
- PartiallyCompleted.
- Cancelled.

### Trigger Types

- Manual.
- Startup.
- Scheduled.

### Relationships

JobSearchProfile has a one-to-many relationship with SearchExecution.

Each SearchExecution references one profile.

Deleting a profile should not silently destroy historical execution records.

A restrict-delete policy or an explicit historical retention strategy is recommended.

## 9. Relationship Diagram

```text
JobPosting
    |
    | 1 : N
    |
JobPostingSource

JobPosting
    |
    | 1 : 0..1
    |
JobApplication

JobSearchProfile
    |
    | 1 : N
    |
SearchExecution
```

## 10. Duplicate Detection Strategy

### Level 1: Provider Identifier

Compare:

- SourceType.
- ExternalId.

A matching pair identifies a confirmed duplicate.

### Level 2: Normalized URL

Compare normalized URL fingerprints.

Verify the corresponding normalized URLs before merging.

### Level 3: Similarity

Compare:

- Company name.
- Job title.
- Location.
- Additional metadata.

Similarity alone does not always prove that two advertisements represent the same opportunity.

### Level 4: Persistence

When a confirmed duplicate is discovered:

1. Locate the existing JobPosting.
2. Add the new JobPostingSource if it does not exist.
3. Update LastSeenAt where appropriate.
4. Do not create another logical JobPosting.

When a potential duplicate is uncertain:

1. Preserve the advertisement separately.
2. Avoid destructive merging.
3. Optionally flag the match for later review.

## 11. Entity Framework Core

The Infrastructure project will contain:

```text
Persistence/
├── JobHunterDbContext.cs
├── Configurations/
└── Migrations/
```

Planned DbSets:

```csharp
public DbSet<JobPosting> JobPostings => Set<JobPosting>();

public DbSet<JobPostingSource> JobPostingSources
    => Set<JobPostingSource>();

public DbSet<JobApplication> JobApplications
    => Set<JobApplication>();

public DbSet<JobSearchProfile> JobSearchProfiles
    => Set<JobSearchProfile>();

public DbSet<SearchExecution> SearchExecutions
    => Set<SearchExecution>();
```

These declarations are illustrative and require the corresponding entity classes.

## 12. Migration Strategy

1. Define Domain entities.
2. Configure entity mappings.
3. Create the initial EF Core migration.
4. Review generated SQL and constraints.
5. Apply the migration to the development database.
6. Run integration tests.
7. Commit migration files to source control.

## 13. Data Retention

Advertisements should be archived rather than automatically deleted.

Search execution history may be retained for troubleshooting and reporting.

Retention periods may be configurable in future versions.

## 14. Security

- Protect connection strings.
- Avoid storing API credentials in database records unless encrypted and necessary.
- Do not log sensitive personal notes.
- Use parameterized database operations.
- Restrict database permissions.
- Back up persistent data before destructive migrations.

## 15. Future Extensions

Potential additional entities:

- JobTag.
- JobPostingTag.
- DuplicateReview.
- ApplicationStatusHistory.
- Notification.
- User.
- ProviderConfiguration.
- SearchSchedule.

These are not required for the initial MVP.
