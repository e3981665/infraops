# Deployment Notes

InfraOps is configured as a Docker-first local demo and portfolio application. This document describes a sensible production direction, but it does not add a deployment pipeline or managed infrastructure.

## Recommended Topology

- **Frontend:** build the Vite React app as static assets and host it on a static hosting platform or behind an HTTPS reverse proxy.
- **API:** run the ASP.NET Core API as a containerized service behind HTTPS, a reverse proxy, and platform-level health checks.
- **Database:** use managed PostgreSQL where possible. Enable automated backups, point-in-time recovery, encryption at rest, and restricted network access.
- **Migrations:** run EF Core migrations as a controlled release step before starting the new API version.

## Required Production Configuration

Replace every development default before deploying outside a local demo:

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings__InfraOps` | PostgreSQL connection string with production credentials |
| `Authentication__Issuer` | Stable production JWT issuer |
| `Authentication__Audience` | Expected frontend/client audience |
| `Authentication__SigningKey` | Strong secret from a secret manager |
| `Cors__AllowedOrigins__0` and subsequent entries | Exact HTTPS frontend origins |
| `RateLimiting__AuthSensitive__PermitLimit` | Login/refresh request limit |
| `RateLimiting__AuthSensitive__WindowSeconds` | Login/refresh rate-limit window |
| `DevelopmentSeed__AdminPassword` | Disable or replace seeded admin credentials |

Do not use `DemoOnly-*`, `infraops_demo_only`, or `LOCAL_DEVELOPMENT_SIGNING_KEY_REPLACE_ME_DEMO_ONLY` in any non-local environment.

## Frontend Notes

- Set `VITE_API_BASE_URL` to the public HTTPS API origin at build time.
- Serve static assets with HTTPS and long-lived cache headers for hashed assets.
- Keep route handling compatible with React Router by serving `index.html` for unknown frontend paths.
- Review token storage before production. The MVP uses `localStorage` for portfolio/demo simplicity; high-risk deployments should consider secure HttpOnly cookie flows.

## API Notes

- Run with `ASPNETCORE_ENVIRONMENT` set to a non-development value.
- Terminate TLS at the platform or reverse proxy, and forward scheme/host headers correctly.
- Keep HSTS enabled for HTTPS environments.
- Keep CORS explicit. Do not use wildcard origins with authenticated requests.
- Keep structured logs and correlation IDs enabled so frontend errors can be matched to API logs.

## Database Notes

- Use a dedicated database user with least-privilege permissions.
- Restrict PostgreSQL network access to the API runtime.
- Schedule backup restore tests, not just backup creation.
- Monitor slow queries and connection pool saturation after real traffic begins.

## Production Hardening Checklist

- Replace all demo credentials and signing keys.
- Configure exact CORS origins.
- Confirm HTTPS and HSTS behavior.
- Confirm logs do not expose tokens, passwords, or refresh tokens.
- Confirm backups and restore procedures.
- Review object-level site/region access before introducing scoped or tenant-like users.
- Run backend tests, frontend tests, Docker validation, Playwright E2E, CodeQL, and dependency vulnerability checks before release.
