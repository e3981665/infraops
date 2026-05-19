# InfraOps

InfraOps is a configurable infrastructure inventory and preventive maintenance platform for enterprise operations teams. It demonstrates a production-minded vertical slice: dynamic asset modeling, preventive checklist execution, validator review, operational dashboarding, audit visibility, Docker-first local development, and automated tests.

Suggested repository tagline:

> Portfolio-grade infrastructure operations platform built with ASP.NET Core Clean Architecture, React, PostgreSQL, and Docker.

## Why This Project Matters

Many infrastructure teams track assets, preventive maintenance, and approval evidence across spreadsheets, email, and disconnected tools. InfraOps models that workflow as a configurable application: administrators define asset types and checklist templates, technicians execute preventive maintenance against inventory items, and validators approve, reject, or request rework with an audit trail.

For reviewers, the project is intended to show more than CRUD screens. It demonstrates how to preserve historical checklist meaning, centralize domain rules, enforce permission-based access, and keep operational reporting separate from workflow mutation.

## Technical Highlights

- Clean Architecture backend with explicit Domain, Application, Infrastructure, and API boundaries.
- Permission-based JWT authentication with refresh token rotation.
- Configurable Entity Types with dynamic inventory field definitions.
- Preventive Templates with ordered sections, checklist item types, numeric bounds, select options, and failure-comment rules.
- Preventive Executions store immutable template snapshots so later template edits do not corrupt historical records.
- Preventive Validation supports approve, reject, and request-rework transitions with auditable history.
- Operational Dashboard exposes projection-based metrics for inventory, executions, validations, and templates.
- React frontend uses route-level lazy loading, TanStack Query, React Hook Form, and Zod.
- Docker Compose runs frontend, API, PostgreSQL, migrations, and demo seed data together.
- Backend and frontend tests cover domain rules, application use cases, API flows, and key UI behavior.

## Architecture Decisions

- **Modular monolith over microservices:** the bounded contexts are clear, but the MVP benefits from one deployable unit, one database, and simpler local operation.
- **Clean Architecture:** domain rules stay independent from EF Core, ASP.NET Core, and React. Controllers remain thin and delegate to application use cases.
- **Dynamic entity model:** inventory can support UPS, Generator, HVAC, and future asset categories without hardcoded screens or schema changes per equipment type.
- **Execution snapshots:** preventive executions duplicate the template structure used at execution time. This costs storage but protects audit and reporting integrity.
- **Projection-based dashboard queries:** dashboard endpoints read aggregated operational facts without loading full aggregates or mutating workflow state.
- **Docker-first development:** the standard demo path avoids machine-specific setup and keeps PostgreSQL, API, and frontend behavior consistent.

More detail is available in [docs/index.md](docs/index.md).

## Screenshots

Screenshots are intentionally represented as placeholders until final capture from the Docker demo environment:

- [docs/screenshots/dashboard.png](docs/screenshots/dashboard.png) - operational dashboard
- [docs/screenshots/inventory-detail.png](docs/screenshots/inventory-detail.png) - dynamic inventory attributes and audit metadata
- [docs/screenshots/preventive-execution.png](docs/screenshots/preventive-execution.png) - technician checklist execution
- [docs/screenshots/preventive-validation.png](docs/screenshots/preventive-validation.png) - validator review and validation history

See [docs/screenshots/README.md](docs/screenshots/README.md) for capture guidance.

## Demo Walkthrough

1. Start the Docker stack.
2. Log in as the admin user and review the dashboard.
3. Open Entity Types to see configurable definitions for UPS, Generator, and HVAC.
4. Open Inventory to inspect dynamic fields, site/region metadata, and audit fields.
5. Open Preventive Templates to review checklist sections and item rules.
6. Log in as the technician and start or resume a preventive execution.
7. Submit a completed execution.
8. Log in as the validator and approve, reject, or request rework.
9. Return to the dashboard to see status and validation metrics update.

## Development-Only Demo Credentials

These credentials are seeded only for local development and portfolio demos. They are not production secrets and must be changed for any non-local environment.

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@infraops.local` | `InfraOps.Admin!123` |
| Technician | `technician@infraops.local` | `InfraOps.Tech!123` |
| Validator | `validator@infraops.local` | `InfraOps.Validator!123` |

## Running Locally With Docker

Prerequisite: Docker Desktop or Docker Engine with Compose support.

```powershell
Copy-Item .env.example .env
docker compose -f .\compose.dev.yml up -d --build
```

Services:

- frontend: `http://localhost:5173`
- api: `http://localhost:8080`
- health check: `http://localhost:8080/health`
- postgres: `localhost:5432`

Stop the stack:

```powershell
docker compose -f .\compose.dev.yml down
```

Reset local demo data by removing the PostgreSQL volume:

```powershell
docker compose -f .\compose.dev.yml down -v
```

The API applies EF Core migrations and idempotent development seed data on startup.

## Testing & Quality

GitHub Actions runs the same core checks on pushes to `main` and pull requests:

- backend restore, Release build, and backend/API tests
- frontend `npm ci`, lint, production build, and React tests
- MCP tooling dependency audit for production dependencies

Backend:

```powershell
.\.tools\dotnet\dotnet.exe build .\backend\InfraOps.sln
.\.tools\dotnet\dotnet.exe test .\backend\InfraOps.sln
```

Docker backend validation:

```powershell
docker compose -f .\compose.dev.yml run --rm api dotnet test InfraOps.sln
```

Frontend:

```powershell
cd .\frontend
npm install
npm run build
npm run test
```

Quality coverage includes:

- domain tests for aggregate invariants and status transitions
- application tests for use-case orchestration and query filters
- API tests for protected endpoints and workflow behavior
- frontend tests for protected routes, dynamic forms, dashboard rendering, and validation actions
- Vite production build with route-level code splitting to avoid an oversized initial bundle

## Current MVP Scope

- Authentication, roles, permissions, and refresh tokens
- Entity Type management with dynamic field definitions
- Inventory management with dynamic attributes
- Preventive Template management by entity type
- Preventive Execution with drafts, submission, answer validation, and immutable template snapshots
- Preventive Validation with approve, reject, request rework, and validation history
- Operational Dashboard and demo seed data
- Audit metadata visibility across operational screens
- Docker development stack and local MCP database inspection server

## Repository Structure

```text
.
|-- backend
|   |-- InfraOps.sln
|   |-- src
|   |   |-- InfraOps.Api
|   |   |-- InfraOps.Application
|   |   |-- InfraOps.Domain
|   |   `-- InfraOps.Infrastructure
|   `-- tests
|       |-- InfraOps.Api.Tests
|       |-- InfraOps.Application.Tests
|       |-- InfraOps.Domain.Tests
|       `-- InfraOps.Infrastructure.Tests
|-- frontend
|   `-- src
|       |-- app
|       |-- components
|       |-- modules
|       `-- shared
|-- docs
|-- tools
|   `-- mcp-db-server
|-- compose.dev.yml
`-- README.md
```

## Stack

Backend:

- ASP.NET Core Web API
- .NET 8 / C#
- EF Core
- PostgreSQL
- FluentValidation
- xUnit

Frontend:

- React
- TypeScript
- Vite
- React Router
- TanStack Query
- React Hook Form
- Zod
- Vitest and React Testing Library

Tooling:

- Docker Compose
- PostgreSQL 16
- Local stdio MCP server for read-only schema inspection

## Documentation

- [Documentation Index](docs/index.md)
- [Architecture](docs/architecture.md)
- [Domain Model](docs/domain-model.md)
- [Execution and Validation Flow](docs/execution-flow.md)
- [MCP Database Server](tools/mcp-db-server/README.md)

## MCP Database Inspection

The local MCP server is optional and intended for safe PostgreSQL schema inspection during development. It exposes read-only schema/query tools and rejects mutating SQL.

Project-level example: [.codex/config.toml.example](.codex/config.toml.example)

Manual run:

```powershell
cd .\tools\mcp-db-server
npm install
$env:DATABASE_URL = "postgresql://infraops:infraops@localhost:5432/infraops"
$env:DB_SCHEMA = "public"
npm start
```

## Environment Files

- `.env.example` contains development-only Docker defaults.
- `frontend/.env.example` contains the frontend API URL default.
- `tools/mcp-db-server/.env.example` contains local MCP database inspection defaults.

Never commit real environment files, production connection strings, production JWT signing keys, or private credentials.

## Roadmap

Realistic next improvements:

- explicit rework resubmission flow after validator feedback
- email or in-app notifications for submitted and rework-requested executions
- richer dashboard filters and exportable operational reports
- attachment/evidence metadata for checklist failures
- user and role administration screens beyond seeded demo users
- CI workflow for backend tests, frontend tests, and production build
- production deployment profile with managed PostgreSQL and secret storage

## License

This repository currently includes a license placeholder. Before publishing or accepting contributions, choose a final license such as MIT for broad portfolio reuse or Apache-2.0 if explicit patent language is desired.
