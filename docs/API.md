
# JobHunter - REST API Specification

## 1. Overview

The JobHunter API exposes HTTP endpoints for managing job advertisements, application statuses, search profiles, and background searches.

Framework: ASP.NET Core Web API (.NET 10).

Architecture: Controller-based REST API.

The endpoints described in this document are planned contracts. Their implementation status must be verified against the source code.

## 2. General Conventions

### Base Path

```text
/api
```

### Data Format

Requests and responses use JSON.

### Identifiers

Resources use GUID identifiers.

### Timestamps

Timestamps use ISO 8601 formatting.

UTC timestamps are preferred for storage and transport.

### Authentication

The initial local development version may operate without authentication.

Authentication and authorization must be implemented before deployment to an untrusted network or public hosting environment.

## 3. HTTP Status Codes

| Code | Meaning |
|---|---|
| 200 | Successful request |
| 201 | Resource created |
| 204 | Successful request without response body |
| 400 | Invalid request |
| 401 | Authentication required |
| 403 | Access denied |
| 404 | Resource not found |
| 409 | Resource conflict |
| 429 | Too many requests |
| 500 | Unexpected server error |
| 503 | External dependency unavailable |

## 4. Job Advertisement Endpoints

### GET /api/jobs

Returns job advertisements.

Planned query parameters:

| Parameter | Type | Description |
|---|---|---|
| keyword | string | Search title or description |
| company | string | Filter company |
| location | string | Filter location |
| remote | bool | Remote work filter |
| source | string | Source filter |
| status | string | Application status |
| page | int | Page number |
| pageSize | int | Results per page |

Example:

```http
GET /api/jobs?keyword=dotnet&location=Budapest&page=1&pageSize=20
```

Successful response:

```json
{
  "items": [
    {
      "id": "f64f3bc4-1d48-4ecb-bb41-3d3acb92d11e",
      "title": "Junior .NET Developer",
      "company": "Example Company",
      "location": "Budapest",
      "url": "https://example.com/jobs/123",
      "source": "Manual",
      "isRemote": false,
      "status": "New"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1
}
```

This response represents the proposed paginated contract.

### GET /api/jobs/{id}

Returns one job advertisement.

Possible responses:

- 200: Advertisement found.
- 404: Advertisement not found.

### POST /api/jobs

Creates a job advertisement manually.

Example request:

```json
{
  "title": "Junior C# Developer",
  "company": "Example Company",
  "location": "Budapest",
  "description": "Junior software development position.",
  "url": "https://example.com/jobs/456",
  "source": "Manual",
  "isRemote": false,
  "salaryMin": null,
  "salaryMax": null,
  "publishedAt": null
}
```

Expected responses:

- 201: Advertisement created.
- 400: Validation error.
- 409: Duplicate advertisement.

### PATCH /api/jobs/{id}/status

Updates the application status.

Example request:

```json
{
  "status": "Applied"
}
```

Supported status values:

- New
- Saved
- Applied
- Interview
- Rejected
- OfferReceived

Expected responses:

- 200 or 204: Updated.
- 400: Invalid status.
- 404: Advertisement not found.

### DELETE /api/jobs/{id}

Deletes or archives a job advertisement.

The final behavior must be defined before implementation.

Possible responses:

- 204: Operation successful.
- 404: Advertisement not found.

## 5. Search Profile Endpoints

### GET /api/search-profiles

Returns configured search profiles.

### POST /api/search-profiles

Creates a new search profile.

Example:

```json
{
  "name": "Junior .NET Jobs",
  "keywords": "junior C# .NET developer",
  "location": "Budapest",
  "includeRemote": true,
  "isEnabled": true
}
```

### PUT /api/search-profiles/{id}

Updates an existing search profile.

### DELETE /api/search-profiles/{id}

Removes a search profile.

## 6. Search Execution Endpoints

### POST /api/search/run

Triggers a manual search.

The operation should execute through an application service.

Long-running searches may return an execution identifier rather than waiting for all providers to finish.

### GET /api/search/history

Returns recent search executions.

### GET /api/search/history/{id}

Returns details for a specific execution.

## 7. Dashboard Endpoint

### GET /api/dashboard/summary

Returns aggregated application statistics.

Example:

```json
{
  "totalJobs": 120,
  "newJobs": 35,
  "savedJobs": 18,
  "appliedJobs": 12,
  "interviews": 3,
  "offersReceived": 1
}
```

All numbers above are illustrative.

## 8. Validation

API requests shall validate:

- Required properties.
- String lengths.
- Valid URLs.
- Supported enumeration values.
- Numeric ranges.
- Valid pagination parameters.

Domain-level validation shall remain independent of HTTP-specific concerns.

## 9. Error Responses

The API should use a consistent error format.

ASP.NET Core ProblemDetails is recommended.

Example:

```json
{
  "type": "about:blank",
  "title": "Conflict",
  "status": 409,
  "detail": "A job advertisement with this URL already exists."
}
```

## 10. API Documentation

OpenAPI support should be configured for development.

The exact documentation endpoint depends on the selected OpenAPI configuration and additional packages.

## 11. Versioning

The initial API may use unversioned routes.

Versioned routes such as /api/v1/jobs may be introduced before public deployment.

## 12. Security

- Validate all incoming data.
- Apply authentication before public exposure.
- Restrict administrative endpoints.
- Protect manual search execution from abuse.
- Do not expose provider credentials.
- Log errors without leaking sensitive information.
