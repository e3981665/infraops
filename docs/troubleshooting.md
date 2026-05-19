# Troubleshooting

This guide covers common local development and demo issues.

## Start Or Rebuild The Stack

```powershell
docker compose -f .\compose.dev.yml up -d --build
```

Services:

- frontend: `http://localhost:5173`
- API: `http://localhost:8080`
- health: `http://localhost:8080/health`
- PostgreSQL: `localhost:5432`

## Inspect Logs

API logs:

```powershell
docker compose -f .\compose.dev.yml logs -f api
```

Frontend logs:

```powershell
docker compose -f .\compose.dev.yml logs -f frontend
```

PostgreSQL logs:

```powershell
docker compose -f .\compose.dev.yml logs -f postgres
```

When reporting an API error, include the `X-Correlation-ID` response header or the `correlationId` value from the error body.

## Reset The Demo Database

This removes the PostgreSQL volume and reseeds demo data on next API startup.

```powershell
docker compose -f .\compose.dev.yml down -v
docker compose -f .\compose.dev.yml up -d --build
```

To seed Portuguese business labels for a Brazil-focused demo, set the locale before reseeding:

```powershell
$env:INFRAOPS_DEMO_CONTENT_LOCALE = "pt-BR"
docker compose -f .\compose.dev.yml down -v
docker compose -f .\compose.dev.yml up -d --build
```

The setting only affects newly seeded demo data. Existing PostgreSQL volumes keep the labels they already contain.

## Run Tests

Backend through Docker:

```powershell
docker compose -f .\compose.dev.yml run --rm api dotnet test InfraOps.sln
```

Frontend:

```powershell
cd .\frontend
npm ci
npm run lint
npm test -- --run
npm run build
```

End-to-end:

```powershell
docker compose -f .\compose.dev.yml up -d --build
cd .\frontend
npx playwright install chromium
npm run test:e2e
```

## Health Checks

```powershell
Invoke-WebRequest -UseBasicParsing http://localhost:8080/health
Invoke-WebRequest -UseBasicParsing http://localhost:5173
```

## Development Credentials

These are intentionally fake development-only seeded users. Generic secret scanners may report them as password-like strings; they are local demo credentials, not production secrets.

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@infraops.local` | `DemoOnly-Admin-Local` |
| Technician | `technician@infraops.local` | `DemoOnly-Tech-Local` |
| Validator | `validator@infraops.local` | `DemoOnly-Validator-Local` |

Do not use these values outside the local/demo environment.

## Environment Variables

Common Docker variables:

| Variable | Purpose |
| --- | --- |
| `POSTGRES_DB` | Local PostgreSQL database name |
| `POSTGRES_USER` | Local PostgreSQL username |
| `POSTGRES_PASSWORD` | Local PostgreSQL password |
| `POSTGRES_PORT` | Host PostgreSQL port |
| `API_PORT` | Host API port |
| `FRONTEND_PORT` | Host frontend port |
| `INFRAOPS_JWT_SIGNING_KEY` | Development JWT signing key override |
| `INFRAOPS_ADMIN_EMAIL` | Development admin email override |
| `INFRAOPS_ADMIN_PASSWORD` | Development admin password override |
| `INFRAOPS_DEMO_CONTENT_LOCALE` | Optional demo business-label locale. Use `pt-BR` before first seed for Portuguese labels |
| `VITE_API_BASE_URL` | Frontend API base URL |

Never commit real production connection strings, signing keys, or passwords.

## Common Issues

### API returns 401 after leaving the app open

Refresh tokens may have expired or the database may have been reset. Sign out and sign in again.

### Frontend cannot reach the API

Check:

```powershell
docker compose -f .\compose.dev.yml ps
docker compose -f .\compose.dev.yml logs -f api
```

Confirm `VITE_API_BASE_URL` points to `http://localhost:8080` for browser usage.

### Demo data looks stale or duplicated

Reset the database volume:

```powershell
docker compose -f .\compose.dev.yml down -v
docker compose -f .\compose.dev.yml up -d --build
```

### Playwright tests fail on seeded workflow data

Reset the database volume before rerunning E2E tests. The E2E suite creates uniquely named demo records and expects the seeded UPS/Generator/HVAC data to exist.
