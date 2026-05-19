# InfraOps Documentation

This folder contains the supporting architecture and workflow documentation for InfraOps.

## Start Here

- [Architecture](architecture.md) explains the Clean Architecture boundaries, bounded contexts, modular monolith choice, dashboard query design, and Docker topology.
- [Domain Model](domain-model.md) explains the fixed operational core, configurable layer, aggregate boundaries, execution snapshot strategy, and validation history.
- [Execution and Validation Flow](execution-flow.md) explains the technician and validator workflows from inventory item selection through approval, rejection, or rework request.
- [Observability](observability.md) explains structured logging, correlation IDs, error responses, and future OpenTelemetry readiness.
- [Security](security.md) explains authentication, authorization, refresh tokens, security headers, CORS, rate limiting, dependency scanning, and production hardening.
- [Troubleshooting](troubleshooting.md) lists Docker logs, reset commands, tests, E2E commands, and common local issues.
- [Screenshots](screenshots/README.md) lists the expected portfolio screenshot captures.

## Review Path

For a quick technical review:

1. Read the root [README](../README.md) for the business problem, demo instructions, and testing commands.
2. Review [Architecture](architecture.md) for dependency direction and deployment topology.
3. Review [Domain Model](domain-model.md) for the dynamic entity and execution snapshot decisions.
4. Review [Execution and Validation Flow](execution-flow.md) for the core workflow state transitions.
5. Review [Security](security.md), [Observability](observability.md), and [Troubleshooting](troubleshooting.md) for hardening, operating, and debugging the Docker demo.
