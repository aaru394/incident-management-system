# Incident Management System

A production-style incident/ticket management API built with **ASP.NET Core 10**, modeled on the Sev1–Sev3
incident workflows used at enterprise SaaS platforms (CreditLens-style banking incident tooling). Built to
demonstrate clean architecture, SLA-driven business rules, caching, and JWT auth in a backend service — not a
toy CRUD app.

## Why this project

Incidents move through a state machine (`Open → InProgress → Resolved → Closed`), each severity carries its
own SLA clock, every transition is audited, and hot reads are cached — the same shape of problem as production
incident tooling for banking/SaaS platforms.

## Architecture

Clean/onion architecture with strict dependency direction (`Api → Infrastructure → Application → Domain`):

```
src/
  IncidentManagement.Domain          # Entities, enums, domain rules — zero external dependencies
  IncidentManagement.Application     # DTOs, service interfaces/implementations, FluentValidation validators
  IncidentManagement.Infrastructure  # EF Core (PostgreSQL), Redis cache, JWT, BCrypt password hashing
  IncidentManagement.Api             # Controllers, middleware, DI wiring, Swagger
tests/
  IncidentManagement.Tests           # xUnit + Moq unit tests for domain rules and application services
```

**Domain layer owns the business rules.** `Incident.ChangeStatus()` enforces a status-transition state machine
and records every change to an audit trail (`IncidentStatusChange`). SLA deadlines are computed from severity
at creation time (Sev1: 4h, Sev2: 24h, Sev3: 72h) and `IsSlaBreached` is a computed property, not a stored flag.

## Tech stack

| Concern            | Choice                                   |
|---------------------|-------------------------------------------|
| API                 | ASP.NET Core 10 Web API, Controllers      |
| Database            | PostgreSQL via EF Core (Npgsql)           |
| Caching             | Redis (StackExchange.Redis)               |
| Auth                | JWT bearer tokens, BCrypt password hashing |
| Validation          | FluentValidation                          |
| Logging             | Serilog (structured, console sink)        |
| Docs                | Swagger / OpenAPI with bearer auth support |
| Tests               | xUnit, Moq, EF Core InMemory              |
| Containerization    | Docker, docker-compose                    |

## Running locally

### With Docker (recommended)

```bash
docker compose up --build
```

This starts PostgreSQL, Redis, and the API (migrations apply automatically on startup in Development).
API is available at `http://localhost:8080`, Swagger UI at `http://localhost:8080/swagger`.

### Without Docker

1. Start PostgreSQL and Redis locally (or point the connection strings in `appsettings.json` at existing
   instances).
2. Update `Jwt:Secret` in `src/IncidentManagement.Api/appsettings.json` to a real random secret.
3. Run:

```bash
dotnet run --project src/IncidentManagement.Api
```

## Running tests

```bash
dotnet test
```

## API overview

| Method | Route                              | Auth        | Description                          |
|--------|-------------------------------------|-------------|---------------------------------------|
| POST   | `/api/auth/register`               | —           | Register a new user                   |
| POST   | `/api/auth/login`                  | —           | Login, returns JWT                    |
| POST   | `/api/incidents`                   | Bearer      | Create an incident                    |
| GET    | `/api/incidents/{id}`              | Bearer      | Get incident detail (cached in Redis) |
| GET    | `/api/incidents`                   | Bearer      | Query incidents (filter + paginate)   |
| POST   | `/api/incidents/{id}/assign`       | Engineer/Admin | Assign an incident, auto-moves to InProgress |
| PATCH  | `/api/incidents/{id}/status`       | Engineer/Admin | Transition status (validated state machine) |
| POST   | `/api/incidents/{id}/comments`     | Bearer      | Add a comment to an incident          |

Full request/response contracts are in Swagger UI once the API is running.

## Design notes

- **Cache-aside pattern**: `GET /api/incidents/{id}` reads from Redis first; writes invalidate the specific key.
  Redis being down degrades gracefully to direct DB reads (see `RedisCacheService`) rather than failing requests.
- **Status transitions are a whitelist**, not a blacklist — invalid transitions throw a `DomainException`,
  mapped to `400 Bad Request` via `ExceptionHandlingMiddleware`.
- **Audit trail**: every status change is recorded in `IncidentStatusChange`, so "who changed what, when" is
  always answerable — the same requirement incident-management tooling has for compliance.
