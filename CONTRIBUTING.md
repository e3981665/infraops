# Contributing

InfraOps is primarily a portfolio project, but contributions should follow the same standards expected from a production-minded enterprise application.

## Local Setup

Use the Docker development stack unless a task explicitly requires non-container execution:

```powershell
Copy-Item .env.example .env
docker compose -f .\compose.dev.yml up -d --build
```

## Quality Bar

- Keep changes focused and reviewable.
- Preserve Clean Architecture boundaries.
- Keep controllers thin and business rules in Domain/Application.
- Add or update tests for business behavior.
- Do not commit real secrets, production credentials, local logs, database volumes, build output, or editor-specific files.
- Use English, generic domain language only.

## Validation

Backend:

```powershell
.\.tools\dotnet\dotnet.exe build .\backend\InfraOps.sln
.\.tools\dotnet\dotnet.exe test .\backend\InfraOps.sln
```

Frontend:

```powershell
cd .\frontend
npm run build
npm run test
```

Docker backend validation:

```powershell
docker compose -f .\compose.dev.yml run --rm api dotnet test InfraOps.sln
```

## Public Repository Safety

Before opening a pull request or publishing a branch, verify that only source files, documentation, migrations, tests, and safe examples are included. `.env.example` files may contain development-only placeholder values; real `.env` files must remain local.

