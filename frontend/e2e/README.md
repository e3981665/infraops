# InfraOps E2E Tests

These Playwright tests run against the Docker development stack and seeded demo data.

```powershell
docker compose -f ..\compose.dev.yml up -d --build
npm run test:e2e
```

Use `npm run test:e2e:headed` or `npm run test:e2e:ui` for local debugging.
