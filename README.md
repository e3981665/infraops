# InfraOps

InfraOps is a configurable infrastructure inventory and preventive maintenance platform for enterprise operations teams. It demonstrates a production-minded vertical slice: dynamic asset modeling, preventive checklist execution, validator review, operational dashboarding, audit visibility, Docker-first local development, and automated tests.

Suggested repository tagline:

> Portfolio-grade infrastructure operations platform built with ASP.NET Core Clean Architecture, React, PostgreSQL, and Docker.

## Why This Project Matters

Many infrastructure teams track assets, preventive maintenance, and approval evidence across spreadsheets, email, and disconnected tools. InfraOps models that workflow as a configurable application: administrators define asset types and checklist templates, technicians execute preventive maintenance against inventory items, and validators approve, reject, or request rework with an audit trail.

For reviewers, the project is intended to show more than CRUD screens. It demonstrates how to preserve historical checklist meaning, centralize domain rules, enforce permission-based access, and keep operational reporting separate from workflow mutation.

## Technical Highlights

- Clean Architecture backend with explicit Domain, Application, Infrastructure, and API boundaries.
- Permission-based JWT authentication with hashed, rotating refresh tokens.
- Configurable Entity Types with dynamic inventory field definitions.
- Preventive Templates with ordered sections, checklist item types, numeric bounds, select options, and failure-comment rules.
- Preventive Executions store immutable template snapshots so later template edits do not corrupt historical records.
- Preventive Validation supports approve, reject, and request-rework transitions with auditable history.
- Operational Dashboard exposes projection-based metrics for inventory, executions, validations, and templates.
- React frontend uses route-level lazy loading, TanStack Query, React Hook Form, and Zod.
- Docker Compose runs frontend, API, PostgreSQL, migrations, and demo seed data together.
- Security hardening includes safe error responses, correlation IDs, auth rate limiting, conservative API security headers, Dependabot, and CodeQL.
- Backend and frontend tests cover domain rules, application use cases, API flows, and key UI behavior.

## Frontend Visual System

InfraOps uses a compact enterprise operations style inspired by classic admin consoles: a dark navy workspace sidebar, light-gray content canvas, white data cards, strong table headers, restrained blue/teal accents, and reusable state surfaces for loading, empty, and error views. Shared CSS primitives such as `module-panel`, `data-table`, `metric-card`, `status-chip`, `field`, and `status-panel` keep dashboard, inventory, template, execution, and validation screens visually consistent without duplicating page-specific styling.

## Architecture Decisions

- **Modular monolith over microservices:** the bounded contexts are clear, but the MVP benefits from one deployable unit, one database, and simpler local operation.
- **Clean Architecture:** domain rules stay independent from EF Core, ASP.NET Core, and React. Controllers remain thin and delegate to application use cases.
- **Dynamic entity model:** inventory can support UPS, Generator, HVAC, and future asset categories without hardcoded screens or schema changes per equipment type.
- **Execution snapshots:** preventive executions duplicate the template structure used at execution time. This costs storage but protects audit and reporting integrity.
- **Projection-based dashboard queries:** dashboard endpoints read aggregated operational facts without loading full aggregates or mutating workflow state.
- **Docker-first development:** the standard demo path avoids machine-specific setup and keeps PostgreSQL, API, and frontend behavior consistent.

More detail is available in [docs/index.md](docs/index.md), including the [security model](docs/security.md).

## Screenshots

Captured from the Docker demo stack with seeded data. Re-capture guidance is available in [docs/screenshots](docs/screenshots/README.md).

| Operational dashboard | Dark mode dashboard |
| --- | --- |
| ![InfraOps dashboard in light theme](docs/screenshots/dashboard-light-en.png) | ![InfraOps dashboard in dark theme](docs/screenshots/dashboard-dark-en.png) |

| Portuguese dashboard | Entity type builder |
| --- | --- |
| ![InfraOps dashboard in Portuguese](docs/screenshots/dashboard-pt-br.png) | ![Dynamic entity type builder](docs/screenshots/entity-type-builder.png) |

| Inventory form | Preventive template builder |
| --- | --- |
| ![Inventory form with dynamic fields](docs/screenshots/inventory-form.png) | ![Preventive template checklist builder](docs/screenshots/preventive-template-builder.png) |

| Preventive execution | Validation queue |
| --- | --- |
| ![Preventive execution checklist](docs/screenshots/preventive-execution.png) | ![Preventive validation queue](docs/screenshots/validation-queue.png) |

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

These credentials are intentionally fake and seeded only for local development and portfolio demos. They are not production secrets, may be reported by generic secret scanners, and must be changed for any non-local environment.

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@infraops.local` | `DemoOnly-Admin-Local` |
| Technician | `technician@infraops.local` | `DemoOnly-Tech-Local` |
| Validator | `validator@infraops.local` | `DemoOnly-Validator-Local` |

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

Inspect runtime logs:

```powershell
docker compose -f .\compose.dev.yml logs -f api
docker compose -f .\compose.dev.yml logs -f frontend
```

Stop the stack:

```powershell
docker compose -f .\compose.dev.yml down
```

Reset local demo data by removing the PostgreSQL volume:

```powershell
docker compose -f .\compose.dev.yml down -v
```

The API applies EF Core migrations and idempotent development seed data on startup.

### Optional Portuguese Demo Seed Content

Seeded business data is stored as normal editable data. The default seed remains English for CI stability and bilingual portfolio review. For a Brazil-focused local demo, set `INFRAOPS_DEMO_CONTENT_LOCALE=pt-BR` before the first startup, then reset the database volume so the API can reseed Portuguese template, checklist, field, option, and validation-history labels.

```powershell
$env:INFRAOPS_DEMO_CONTENT_LOCALE = "pt-BR"
docker compose -f .\compose.dev.yml down -v
docker compose -f .\compose.dev.yml up -d --build
```

API responses include `X-Correlation-ID`. Error responses also include `correlationId` so a UI error can be matched to API logs. See [docs/observability.md](docs/observability.md) and [docs/troubleshooting.md](docs/troubleshooting.md).

## Security

InfraOps uses permission-based API authorization, strict JWT validation, PBKDF2 password hashes, hashed refresh tokens, refresh-token rotation, safe error responses, auth endpoint rate limiting, explicit local CORS origins, and conservative API security headers. GitHub security automation includes Dependabot and CodeQL.

The frontend stores tokens in `localStorage` for MVP/demo simplicity. That tradeoff is documented along with production alternatives such as secure HttpOnly cookies. Site/region scoping is represented in the domain, but full object-level scoped access enforcement is documented as a production follow-up.

Read [docs/security.md](docs/security.md) before deploying outside a local demo environment.

## Testing & Quality

GitHub Actions runs the core quality gates on pull requests and pushes to `main` or `master`:

- **Backend CI:** `dotnet restore`, `dotnet build`, and `dotnet test` for `backend/InfraOps.sln`
- **Frontend CI:** `npm ci`, lint, Vitest/React tests, and Vite production build from `frontend`
- **Docker CI:** `docker compose -f compose.dev.yml config` and `docker compose -f compose.dev.yml build`
- **E2E CI:** Docker stack startup, API/frontend readiness checks, and Playwright browser tests
- **CodeQL:** C# and JavaScript/TypeScript static analysis
- **Dependabot:** weekly NuGet, npm, GitHub Actions, and Docker dependency update checks

These workflows require no deployment secrets and are intended as branch-protection gates for public portfolio review.

Backend:

```powershell
dotnet restore .\backend\InfraOps.sln
dotnet build .\backend\InfraOps.sln --no-restore
dotnet test .\backend\InfraOps.sln --no-build
```

Docker backend validation:

```powershell
docker compose -f .\compose.dev.yml config
docker compose -f .\compose.dev.yml build
docker compose -f .\compose.dev.yml run --rm api dotnet test InfraOps.sln
docker compose -f .\compose.dev.yml run --rm api dotnet list InfraOps.sln package --vulnerable --include-transitive
```

Frontend:

```powershell
cd .\frontend
npm ci
npm run lint
npm test -- --run
npm run build
npm audit
```

End-to-end browser tests:

```powershell
docker compose -f .\compose.dev.yml up -d --build
cd .\frontend
npx playwright install chromium
npm run test:e2e
```

Debug E2E tests locally:

```powershell
npm run test:e2e:headed
npm run test:e2e:ui
```

The Playwright suite uses the development/demo credentials above and expects seeded data from the Docker API startup. The GitHub Actions E2E workflow starts the Docker stack, waits for `http://localhost:8080/health` and `http://localhost:5173`, runs Chromium tests, uploads the Playwright report, and tears the stack down.

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
- [Observability](docs/observability.md)
- [Security](docs/security.md)
- [Deployment Notes](docs/deployment.md)
- [Troubleshooting](docs/troubleshooting.md)
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
- `INFRAOPS_DEMO_CONTENT_LOCALE=pt-BR` can be set before a fresh seed for Portuguese demo business labels.

Never commit real environment files, production connection strings, production JWT signing keys, or private credentials.

## Roadmap

Realistic next improvements:

- explicit rework resubmission flow after validator feedback
- email or in-app notifications for submitted and rework-requested executions
- richer dashboard filters and exportable operational reports
- attachment/evidence metadata for checklist failures
- user and role administration screens beyond seeded demo users
- production deployment profile with managed PostgreSQL and secret storage

## License

InfraOps is released under the [MIT License](LICENSE).
