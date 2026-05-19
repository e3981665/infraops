# Security

InfraOps is an MVP portfolio application, but it is structured to demonstrate production-minded security decisions: server-side permission checks, short-lived access tokens, refresh token rotation, safe errors, correlation IDs, and Docker-first development defaults.

## Authentication

- Users authenticate with email and password.
- Passwords are hashed with PBKDF2-SHA256 using per-password random salts.
- Access tokens are JWT bearer tokens with issuer, audience, signing-key, and lifetime validation enabled.
- Access tokens expire after 15 minutes by default.
- Refresh tokens are generated with cryptographically secure randomness.
- Refresh tokens are hashed before persistence.
- Refresh tokens rotate on use. The previous refresh token is revoked and linked to the replacement token hash.
- Logout revokes the submitted refresh token.
- Inactive users cannot log in or refresh tokens.

Development seed credentials are documented in the root README and are for local/demo use only. They use obvious `DemoOnly-*` values so generic secret-scanning findings can be classified as intentional demo credentials, not leaked production secrets. Replace all JWT signing keys, seed credentials, and database credentials for any non-local environment.

## Authorization

- API authorization is permission-based.
- Controllers use authorization policies such as `inventory.read`, `preventive.execute`, and `preventive.validate`.
- Frontend route guards are user experience only. The API remains the source of truth for authorization.
- Object-level access currently enforces existence, status, and permission checks. Fine-grained site/region data scoping is modeled in the domain but is not fully enforced as a production tenant boundary in this MVP.

Production follow-up:

- enforce site/region scope in every ID-based query and command where scoped users are introduced
- add explicit regression tests for cross-site and cross-region access denial
- review every new endpoint for function-level and object-level authorization before merging

## Input Validation

- API requests are validated at the application boundary with FluentValidation.
- Domain aggregates enforce workflow invariants such as preventive execution status transitions and validation action rules.
- Dynamic inventory values are validated against entity field definitions.
- Preventive execution answers are validated against the execution template snapshot, including required answers, numeric bounds, select options, yes/no values, and comment-on-failure rules.
- Controllers remain thin and do not contain business rules.

## Refresh Token Lifecycle

Refresh token storage intentionally keeps only hashes. A stolen database record is not directly usable as a refresh token. Refresh requests must present the raw token, which is hashed and matched server-side. On successful refresh, the old token is revoked and a new token is issued.

Recommended production hardening:

- monitor repeated refresh failures
- shorten refresh token lifetime for higher-risk deployments
- add device/session management if users need visibility into active sessions

## Rate Limiting

The API uses ASP.NET Core rate limiting for sensitive authentication endpoints:

- `POST /api/auth/login`
- `POST /api/auth/refresh`

Defaults:

- `RateLimiting__AuthSensitive__PermitLimit=30`
- `RateLimiting__AuthSensitive__WindowSeconds=60`

These values are intentionally reasonable for development demos. Production deployments should tune them based on expected traffic, reverse proxy behavior, and monitoring.

## Security Headers

API responses include conservative security headers:

- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Referrer-Policy: no-referrer`
- `Permissions-Policy: camera=(), microphone=(), geolocation=()`
- `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'; base-uri 'none'`

HSTS is enabled outside Development environments. The CSP is intentionally API-oriented. If the API begins serving browser assets directly, review the CSP before shipping.

## CORS

CORS allows explicit local frontend origins by default:

- `http://localhost:5173`
- `http://127.0.0.1:5173`
- `http://localhost:4173`
- `http://127.0.0.1:4173`

Production deployments must configure `Cors__AllowedOrigins` with the exact frontend origin. Do not use wildcard CORS for authenticated environments.

## Error Handling and Logging

- Global exception handling returns safe error responses.
- Stack traces are not returned to clients.
- API responses include `X-Correlation-ID`.
- Error responses include `correlationId` so frontend errors can be matched to API logs.
- Structured logs include request lifecycle events and important workflow actions.
- Logs must never include passwords, raw refresh tokens, access tokens, or secrets.

## Frontend Token Handling

The React frontend stores access and refresh tokens in `localStorage` for MVP simplicity and deterministic demo behavior. This is a conscious tradeoff: it is easy to run and test, but it increases exposure if an XSS vulnerability exists.

Current mitigations:

- no `dangerouslySetInnerHTML` usage was found in the frontend source
- frontend route protection is not treated as authorization
- API errors do not log tokens
- security headers are applied by the API

Production alternatives:

- use secure, HttpOnly, SameSite cookies for refresh tokens
- add a stricter frontend CSP at the hosting layer
- add automated XSS-focused UI review for any rich text or HTML-rendering feature

## Docker and Local Development

- `.env` files are ignored and should not be committed.
- `.env.example` contains development-only defaults and warnings.
- PostgreSQL credentials in Docker Compose are local/demo credentials.
- Docker logs are visible with:

```powershell
docker compose -f .\compose.dev.yml logs -f api
docker compose -f .\compose.dev.yml logs -f frontend
```

Reset local demo data:

```powershell
docker compose -f .\compose.dev.yml down -v
docker compose -f .\compose.dev.yml up -d --build
```

## Dependency and Code Scanning

The repository includes:

- Dependabot updates for NuGet, npm, GitHub Actions, and Docker files.
- CodeQL analysis for C# and JavaScript/TypeScript.
- CI build/test workflows for backend, frontend, Docker, and E2E tests.

Local security checks:

```powershell
docker compose -f .\compose.dev.yml run --rm api dotnet list InfraOps.sln package --vulnerable --include-transitive

cd .\frontend
npm audit
```

`npm audit` is informational by default. Review high or critical vulnerabilities promptly, especially when they affect production runtime dependencies.

## Secret Scanning Triage

Expected public-repository findings:

- `DemoOnly-Admin-Local`, `DemoOnly-Tech-Local`, and `DemoOnly-Validator-Local` are seeded local demo passwords.
- `LOCAL_DEVELOPMENT_SIGNING_KEY_REPLACE_ME_DEMO_ONLY` is a fake development JWT signing key for local startup only.
- `infraops_demo_only` is the local PostgreSQL password used by Docker Compose.

These values are safe to publish because they are intentionally fake and scoped to local demo infrastructure. Treat any other password, token, API key, private key, production connection string, or non-demo signing key as a real incident: revoke it, remove it from history where appropriate, and rotate the affected service credential.

## Production Hardening Checklist

- Replace all development secrets and demo credentials.
- Use a strong JWT signing key from a secret manager.
- Configure exact production CORS origins.
- Enforce HTTPS at the edge and keep HSTS enabled.
- Move refresh tokens to secure HttpOnly cookies if the frontend deployment model supports it.
- Add site/region object-level authorization for scoped users.
- Add centralized log retention and alerting.
- Review rate limit thresholds behind the chosen reverse proxy.
- Enable database backups and migration rollback procedures.
- Keep Dependabot, CodeQL, backend tests, frontend tests, and E2E checks as branch protection gates.
