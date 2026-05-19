# Publishing Checklist

Use this checklist before sharing InfraOps publicly or linking it from a portfolio.

## Before Publishing

- Confirm `LICENSE` contains the intended final license text.
- Confirm demo credentials are clearly marked development-only.
- Confirm `.env`, local database files, build output, test reports, screenshots from failed tests, and logs are not committed.
- Confirm `.env.example`, `frontend/.env.example`, and `tools/mcp-db-server/.env.example` contain only development-safe defaults.
- Confirm `README.md` has the project overview, demo flow, setup commands, testing commands, docs links, roadmap, and license.
- Confirm `docs/security.md`, `docs/observability.md`, and `docs/troubleshooting.md` are current.

## Screenshot Checklist

Capture real screenshots from the Docker demo stack. Do not use mock or generated screenshots.

- `docs/screenshots/dashboard-light-en.png`
- `docs/screenshots/dashboard-dark-en.png`
- `docs/screenshots/dashboard-pt-br.png`
- `docs/screenshots/entity-type-builder.png`
- `docs/screenshots/inventory-form.png`
- `docs/screenshots/preventive-template-builder.png`
- `docs/screenshots/preventive-execution.png`
- `docs/screenshots/validation-queue.png`

## Validation Checklist

- `docker compose -f compose.dev.yml down -v`
- `docker compose -f compose.dev.yml up -d --build`
- `http://localhost:8080/health` returns healthy.
- `http://localhost:5173` loads the frontend.
- Demo login works with `admin@infraops.local`.
- Dashboard shows seeded metrics.
- `npm run test:e2e` passes from `frontend`.
- Backend restore/build/tests pass.
- Frontend lint/tests/build pass.
- `docker compose -f compose.dev.yml config` passes.
- `npm audit --audit-level=moderate` passes.
- `dotnet list package --vulnerable --include-transitive` reports no known vulnerabilities.

## GitHub Checks

- Repository is public.
- Repository description and topics are set.
- Actions are enabled.
- Backend CI, Frontend CI, Docker CI, CodeQL, and E2E CI are visible.
- Dependabot is enabled.
- Branch protection is enabled after workflows are stable.
- Wiki and Projects are disabled unless intentionally used.

## Deployment Checklist

- Replace all development credentials and JWT signing keys.
- Configure exact production CORS origins.
- Use managed PostgreSQL or a backed-up PostgreSQL deployment.
- Keep HSTS enabled behind HTTPS.
- Review token storage strategy for the chosen frontend hosting model.
- Add site/region object-level authorization before multi-tenant or scoped-user production use.
- Configure log retention, alerting, backup, and restore procedures.

## Portfolio Checklist

- Add the repository URL to the portfolio.
- Add two or three screenshots with captions.
- Link directly to architecture, security, and execution-flow docs.
- Mention the stack: ASP.NET Core, React, TypeScript, PostgreSQL, Docker, GitHub Actions, Playwright.
- Summarize the business problem in one sentence.
- Summarize the strongest engineering decisions: Clean Architecture, dynamic entity model, execution snapshots, validation history, and CI/security hardening.
