# Publishing Checklist

Use this checklist before sharing InfraOps publicly or linking it from a portfolio.

## Repository Readiness

- [x] MIT license file is present.
- [x] README explains the business problem, value proposition, architecture, demo flow, setup, quality gates, and roadmap.
- [x] Demo credentials are clearly marked development-only.
- [x] `.env.example`, `frontend/.env.example`, and `tools/mcp-db-server/.env.example` contain development-safe defaults only.
- [x] Security, observability, troubleshooting, deployment, and architecture docs are present.
- [ ] Confirm no local `.env`, database volumes, logs, Playwright traces, failed-test screenshots, `bin/`, `obj/`, `node_modules/`, or `dist/` artifacts are staged.

## Screenshot Checklist

Real screenshots are stored under `docs/screenshots/` and referenced from the README.

- [x] `docs/screenshots/dashboard-light-en.png`
- [x] `docs/screenshots/dashboard-dark-en.png`
- [x] `docs/screenshots/dashboard-pt-br.png`
- [x] `docs/screenshots/entity-type-builder.png`
- [x] `docs/screenshots/inventory-form.png`
- [x] `docs/screenshots/preventive-template-builder.png`
- [x] `docs/screenshots/preventive-execution.png`
- [x] `docs/screenshots/validation-queue.png`
- [x] Screenshot capture instructions are documented in `docs/screenshots/README.md`.

## Validation Checklist

- [ ] `docker compose -f compose.dev.yml down -v`
- [x] `docker compose -f compose.dev.yml up -d --build`
- [x] `http://localhost:8080/health` returns healthy.
- [x] `http://localhost:5173` loads the frontend.
- [ ] Demo login works manually with `admin@infraops.local`.
- [x] Dashboard shows seeded metrics through E2E/API-backed browser coverage.
- [x] Backend restore/build/tests pass.
- [x] Frontend lint/tests/build pass.
- [x] `npm run test:e2e` passes from `frontend`.
- [x] `docker compose -f compose.dev.yml config` passes.
- [x] `npm audit --audit-level=moderate` reviewed.
- [x] `dotnet list package --vulnerable --include-transitive` reviewed.

## CI And Security

- [x] Backend CI workflow exists.
- [x] Frontend CI workflow exists.
- [x] Docker validation workflow exists.
- [x] E2E CI workflow exists with Docker readiness checks.
- [x] CodeQL workflow exists for C# and JavaScript/TypeScript.
- [x] Dependabot is configured for NuGet, npm, GitHub Actions, Docker, and the .NET SDK.
- [x] Demo credentials were triaged as fake development-only values.
- [ ] Confirm GitHub Actions are green on the final pushed commit.
- [ ] Confirm GitHub CodeQL has no untriaged high-severity alerts in the GitHub Security tab.
- [ ] Confirm GitGuardian findings, if any, are triaged as demo-only credentials or false positives.

## Deployment TODOs

- [ ] Replace all development credentials and JWT signing keys.
- [ ] Configure exact production CORS origins.
- [ ] Use managed PostgreSQL or a backed-up PostgreSQL deployment.
- [ ] Confirm HTTPS termination and HSTS behavior.
- [ ] Review frontend token storage for the chosen hosting model.
- [ ] Enforce site/region object-level authorization before scoped-user or tenant-style production use.
- [ ] Configure log retention, alerting, backup, and restore procedures.
- [ ] Run a staging smoke test before any production deployment.

## Portfolio TODOs

- [ ] Add repository URL to the portfolio site.
- [ ] Add two or three screenshots with captions to the portfolio project page.
- [ ] Link directly to architecture, security, deployment, and execution-flow docs.
- [ ] Mention the stack: ASP.NET Core, React, TypeScript, PostgreSQL, Docker, GitHub Actions, Playwright.
- [ ] Summarize the business problem in one sentence.
- [ ] Summarize the strongest engineering decisions: Clean Architecture, dynamic entity model, execution snapshots, validation history, observability, and CI/security hardening.
- [ ] Prepare a concise LinkedIn/GitHub post with screenshots and a link to the repository.
