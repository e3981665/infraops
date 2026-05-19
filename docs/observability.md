# Observability

InfraOps uses lightweight, local-first observability that is useful in Docker, CI, and portfolio review without requiring an external monitoring vendor.

## Logging Strategy

The API is configured with Serilog console logging. Logs are written to standard output, so Docker and GitHub Actions can collect them directly.

```powershell
docker compose -f .\compose.dev.yml logs -f api
```

Logs use structured message templates with named properties such as:

- `CorrelationId`
- `Method`
- `Path`
- `StatusCode`
- `ElapsedMilliseconds`
- domain identifiers such as `EntityTypeId`, `InventoryItemId`, and `PreventiveExecutionId`

The API logs:

- HTTP request start and completion
- unhandled exceptions
- login success and failure without passwords or tokens
- entity type create/update/activate/deactivate
- inventory create/update/activate/deactivate
- preventive execution submission
- preventive validation approve/reject/request-rework decisions

Sensitive values are intentionally excluded from logs:

- passwords
- access tokens
- refresh tokens
- JWT signing keys
- raw request bodies

## Correlation IDs

Every API request receives a correlation ID.

Header:

```text
X-Correlation-ID
```

Behavior:

- If the client sends `X-Correlation-ID`, the API preserves it.
- If the client does not send one, the API generates one from the ASP.NET Core trace identifier.
- The API returns the value on every response using the same header.
- Structured logs include the same `CorrelationId`.
- Error responses include `correlationId` in the JSON body.

Example error response:

```json
{
  "code": "server_error",
  "message": "An unexpected error occurred.",
  "errors": null,
  "correlationId": "0HN..."
}
```

Frontend API errors preserve this value and append a reference to user-facing error messages when the backend provides it.

## Error Handling

The API uses a global exception middleware that maps known errors to stable response shapes:

| Condition | HTTP status | Code |
| --- | ---: | --- |
| FluentValidation failure | 400 | `validation_error` |
| Domain rule violation | 400 | `domain_rule_violation` |
| Unauthorized application action | 401 | `unauthorized` |
| Missing application resource | 404 | `not_found` |
| Unhandled exception | 500 | `server_error` |

Unhandled exceptions are logged with the exception details server-side, but the response does not expose stack traces.

## OpenTelemetry Readiness

InfraOps does not require an OpenTelemetry collector for local development. The current design is ready for later OTEL integration because:

- HTTP requests have stable correlation IDs.
- logs use structured properties.
- request lifecycle timing is centralized.
- domain workflow actions are logged with stable identifiers.

A future production profile could add `OpenTelemetry.Extensions.Hosting`, ASP.NET Core instrumentation, EF Core instrumentation, and an OTLP exporter behind environment flags such as `OTEL_EXPORTER_OTLP_ENDPOINT`.

## Useful Signals

For the current MVP, the highest-value operational signals are:

- request error rate by endpoint
- request latency by endpoint
- failed login count
- preventive submissions per day
- validations approved/rejected/rework-requested
- database migration or seed failures during startup

These are currently inspectable through logs and dashboard data. They can later become metrics without changing the domain model.
