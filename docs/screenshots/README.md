# Screenshots

This directory contains real portfolio screenshots captured from the Docker demo stack with seeded data.

## Included Assets

| File | View |
| --- | --- |
| `dashboard-light-en.png` | Operational dashboard, light theme, English |
| `dashboard-dark-en.png` | Operational dashboard, dark theme, English |
| `dashboard-pt-br.png` | Operational dashboard, Portuguese |
| `entity-type-builder.png` | Dynamic entity type configuration |
| `inventory-form.png` | Inventory form with dynamic fields |
| `preventive-template-builder.png` | Preventive template and checklist builder |
| `preventive-execution.png` | Technician checklist execution |
| `validation-queue.png` | Validator review queue |

## Capture Environment

- Docker stack running from `compose.dev.yml`
- Browser width: `1366px`
- Freshly seeded development database
- No browser extensions, developer tools, or local overlays visible

## Re-Capture Steps

```powershell
docker compose -f .\compose.dev.yml down -v
docker compose -f .\compose.dev.yml up -d --build
```

Then open `http://localhost:5173`, sign in with the development-only demo users from the root README, and capture the views listed above. Keep screenshots factual: do not use mockups, generated images, or edited UI states that the app cannot render.
