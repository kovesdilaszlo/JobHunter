
# JobHunter - Test Plan

## 1. Introduction

This document defines the testing strategy for the JobHunter application.

The goal is to verify business rules, application services, persistence behavior, API endpoints, and external integrations.

Testing framework: xUnit.

## 2. Testing Objectives

- Detect defects early.
- Validate domain rules.
- Verify application workflows.
- Prevent regressions.
- Ensure database operations behave correctly.
- Validate API responses.
- Test failure handling for external providers.

## 3. Testing Levels

### 3.1 Unit Tests

Unit tests verify individual components in isolation.

Primary targets:

- Domain entities.
- Domain validation.
- Application services.
- Filtering logic.
- Duplicate detection.
- Application status transitions.

External dependencies should be replaced with test doubles where appropriate.

### 3.2 Integration Tests

Integration tests verify interactions between components.

Targets:

- EF Core repositories.
- Database constraints.
- API endpoints.
- Dependency injection.
- Persistence transactions.

Use a test database where relational behavior matters.

EF Core InMemory is not a substitute for SQL Server when validating relational constraints or SQL-specific behavior.

### 3.3 External Integration Tests

External provider tests should verify:

- Request formatting.
- Response parsing.
- Invalid responses.
- Authentication errors.
- Rate limiting.
- Network failures.
- Empty results.

Use mocked HTTP responses for deterministic tests.

Live provider tests should be optional and separately configured.

### 3.4 User Interface Tests

Blazor UI tests should verify:

- Job listing rendering.
- Filtering controls.
- Status updates.
- Navigation.
- Error messages.
- Empty states.

Component testing may use bUnit.

End-to-end browser testing may be added later.

## 4. Domain Test Cases

| ID | Test Case | Expected Result |
|---|---|---|
| D-001 | Create valid job posting | Entity created |
| D-002 | Empty title | Validation failure |
| D-003 | Empty company | Validation failure |
| D-004 | Invalid URL | Validation failure |
| D-005 | Negative salary | Validation failure |
| D-006 | Minimum salary exceeds maximum | Validation failure |
| D-007 | Valid status change | Status updated |
| D-008 | Invalid status value | Validation failure |

## 5. Application Test Cases

| ID | Test Case | Expected Result |
|---|---|---|
| A-001 | Retrieve all jobs | Correct collection |
| A-002 | Retrieve existing job | Correct DTO |
| A-003 | Retrieve missing job | Not found result |
| A-004 | Create valid job | Repository called |
| A-005 | Create duplicate job | Conflict detected |
| A-006 | Update existing status | Successful update |
| A-007 | Update missing job | Not found result |
| A-008 | Cancellation requested | Operation cancelled appropriately |

## 6. Repository Test Cases

| ID | Test Case | Expected Result |
|---|---|---|
| R-001 | Insert advertisement | Record persisted |
| R-002 | Query by identifier | Correct record |
| R-003 | Query missing identifier | Null result |
| R-004 | Duplicate normalized URL | Constraint enforced |
| R-005 | Update advertisement | Changes persisted |
| R-006 | Filter by status | Correct records |
| R-007 | Filter by location | Correct records |

## 7. API Test Cases

| ID | Test Case | Expected Result |
|---|---|---|
| API-001 | GET existing job | HTTP 200 |
| API-002 | GET missing job | HTTP 404 |
| API-003 | POST valid job | HTTP 201 |
| API-004 | POST invalid job | HTTP 400 |
| API-005 | POST duplicate job | HTTP 409 |
| API-006 | PATCH valid status | Success |
| API-007 | PATCH missing job | HTTP 404 |

The expected status codes reflect the planned API contract.

## 8. Background Search Tests

Test scenarios:

1. Search runs at the configured time.
2. Disabled profiles are skipped.
3. Multiple providers are processed.
4. Provider failure is logged.
5. Duplicate advertisements are ignored.
6. New advertisements are saved.
7. Execution statistics are recorded.
8. Cancellation is respected.
9. Repeated execution does not create duplicates.
10. Concurrent execution is handled safely.

## 9. Test Naming Convention

Recommended pattern:

MethodName_Scenario_ExpectedResult

Examples:

```text
CreateAsync_ValidRequest_ReturnsJobDto
CreateAsync_DuplicateUrl_ThrowsConflict
ChangeStatusAsync_ExistingJob_UpdatesStatus
ChangeStatusAsync_MissingJob_ReturnsFalse
```

## 10. Running Tests

From the Solution directory:

```powershell
dotnet test
```

To run a specific project:

```powershell
dotnet test path/to/JobHunter.Test.csproj
```

To run tests using Visual Studio:

Test > Test Explorer > Run All Tests

## 11. Code Coverage

Coverage should focus on meaningful behavior.

High-priority areas:

- Domain validation.
- Application workflows.
- Duplicate detection.
- Status transitions.
- Filtering.
- Background search coordination.

Coverage percentage alone does not prove correctness.

## 12. Test Data

Use deterministic test data.

Tests must not depend on:

- Real user credentials.
- Live job advertisements.
- Production databases.
- External API availability.
- Current system time without an abstraction.

## 13. Continuous Integration

A future GitHub Actions workflow should:

1. Restore dependencies.
2. Build the solution.
3. Execute automated tests.
4. Report failures.
5. Optionally collect coverage.

## 14. Exit Criteria

A release candidate is ready when:

- The solution builds successfully.
- Required unit tests pass.
- Required integration tests pass.
- Critical defects are resolved.
- Core user workflows are validated.
- Documentation reflects implemented behavior.
