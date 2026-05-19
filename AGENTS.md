# AGENTS.md

## Project Name
InfraOps

## Project Overview
InfraOps is a configurable infrastructure inventory and preventive maintenance platform.

The system is designed for enterprise operational environments where users need to:

- authenticate securely
- manage users, roles, and permissions
- configure infrastructure entity types dynamically
- register inventory items based on those entity definitions
- configure preventive maintenance templates per entity type
- execute preventive maintenance checklists
- validate submitted preventive executions

The platform must be built as a modernized, portfolio-safe, productized solution inspired by real-world infrastructure operations systems, but without reproducing proprietary structures or company-specific terminology.

---

## Product Goals
The MVP must support:

1. Authentication
2. Roles and permissions
3. Entity type configuration
4. Inventory management
5. Preventive template management
6. Preventive execution
7. Preventive validation

The system must balance a fixed operational core with a configurable domain layer.

### Fixed Core
These areas are code-defined and not fully dynamic in MVP:

- authentication
- authorization
- users
- roles
- permissions
- regions
- sites
- preventive statuses
- validation statuses
- audit metadata

### Configurable Layer
These areas are configurable by admin users:

- entity types
- entity fields
- preventive templates
- preventive sections
- preventive checklist items
- selected field-level and checklist-level rules

---

## Tech Stack

### Backend
- ASP.NET Core Web API
- C#
- Clean Architecture
- Entity Framework Core
- MediatR or equivalent request/handler pattern
- FluentValidation
- JWT authentication
- xUnit
- Moq or NSubstitute
- Testcontainers or equivalent for integration tests when needed

### Frontend
- React
- TypeScript
- Vite
- React Router
- TanStack Query
- React Hook Form
- Zod
- Component library: prefer shadcn/ui or MUI
- Vitest and React Testing Library

### Database
- PostgreSQL is the default local development database
- Use relational modeling with clear constraints and audit fields

### Local Development
- Docker
- Docker Compose
- PostgreSQL official image for local containerized development
- Local MCP server for safe PostgreSQL schema inspection

Local development should prefer the Docker Compose workflow unless a task explicitly requires a non-container path.

---

## Architecture Principles

### Clean Architecture
Follow strict separation of concerns:

- Domain
- Application
- Infrastructure
- Presentation

Dependencies must always point inward.

#### Domain
Contains:
- entities
- value objects
- enums
- domain services
- domain rules
- domain exceptions

Domain must not depend on:
- EF Core
- ASP.NET Core
- external APIs
- infrastructure details
- UI concerns

#### Application
Contains:
- commands
- queries
- handlers
- DTOs
- interfaces
- validators
- orchestration logic
- authorization requirements at use-case level when appropriate

Application depends on Domain, but not on Infrastructure or Presentation.

#### Infrastructure
Contains:
- EF Core DbContext
- repository implementations
- authentication providers
- file storage implementations
- external service integrations
- logging adapters
- migrations

Infrastructure implements contracts defined by Application or Domain.

#### Presentation
Contains:
- API controllers
- request/response contracts
- middleware
- filters
- authentication setup
- dependency injection setup

Controllers must be thin. Controllers only coordinate HTTP concerns and delegate business behavior to Application.

---

## SOLID Expectations

All generated code must follow SOLID principles.

### Single Responsibility Principle
- Do not create god services.
- Each class should have one clear reason to change.
- Handlers should orchestrate one use case only.

### Open/Closed Principle
- Prefer extensible designs for entity fields, preventive templates, and rule evaluation.
- New entity types and checklist structures should not require controller rewrites.

### Liskov Substitution Principle
- Abstractions must be safely replaceable.
- Do not create inheritance hierarchies unless they are clearly justified.

### Interface Segregation Principle
- Keep interfaces small and focused.
- Avoid large service interfaces with unrelated methods.

### Dependency Inversion Principle
- Application depends on abstractions, not implementations.
- Infrastructure provides implementations for those abstractions.

---

## TDD Requirements

Test-driven development is mandatory for business-critical code.

### Required TDD Flow
For new business behavior:
1. write a failing test
2. implement the minimal code needed
3. refactor while keeping tests green

### Priority for Tests
Highest priority:
- domain rules
- aggregate behavior
- status transitions
- validation rules
- use case handlers

Medium priority:
- repositories
- API endpoint integration
- authorization policies

Lower priority:
- trivial mappings
- passive DTOs
- framework plumbing without custom logic

### Backend Test Types

#### Domain Unit Tests
Test business invariants and domain behavior.

Examples:
- entity type cannot be created without a valid name
- required dynamic fields must be defined correctly
- preventive execution cannot be submitted without required answers
- validation cannot approve an already approved execution
- rework request must include a reason when required

#### Application Unit Tests
Test use-case orchestration.

Examples:
- creating entity types persists valid definitions
- inventory item creation validates dynamic field payloads
- preventive submission changes status and records audit metadata
- validation approval updates status and stores validator info

#### Integration Tests
Test important real flows end-to-end at API or infrastructure level.

Examples:
- login returns JWT and refresh token
- protected endpoints reject unauthorized access
- inventory creation persists fixed and dynamic data
- preventive submission and validation flow behaves correctly

### Test Naming
Use descriptive behavior-based names.

Examples:
- `Should_CreateEntityType_When_RequestIsValid`
- `Should_RejectPreventiveSubmission_When_RequiredChecklistAnswerIsMissing`
- `Should_RequestRework_When_ValidatorRejectsExecutionWithReason`

Do not use vague test names like:
- `TestCreate`
- `ValidationTest1`

---

## Domain Language

Use clean, generic, English domain language.

Prefer these terms:

- Entity Type
- Entity Field Definition
- Inventory Item
- Inventory Attribute Value
- Preventive Template
- Preventive Section
- Preventive Checklist Item
- Preventive Execution
- Preventive Answer
- Preventive Validation
- Region
- Site
- Permission
- Role

Avoid leaking legacy or proprietary internal names.

Do not use company-specific abbreviations unless they are generic domain examples seeded in demo data.

---

## MVP Domain Rules

### Identity and Access
- Users authenticate via JWT.
- Users have one or more roles.
- Roles map to permissions.
- Users may have access scope restrictions by region or site.

### Entity Types
- Admins can create, edit, activate, and deactivate entity types.
- Each entity type has field definitions.
- Field definitions describe how inventory forms should be rendered and validated.
- Supported field types in MVP:
  - text
  - textarea
  - number
  - decimal
  - boolean
  - date
  - select

### Inventory
- Inventory items belong to one entity type.
- Inventory items contain fixed metadata and dynamic attribute values.
- Fixed metadata should include:
  - entity type
  - site
  - region
  - status
  - installation date
  - created by
  - updated by
- Dynamic values must be validated against field definitions.

### Preventive Templates
- Preventive templates are linked to entity types.
- A template contains sections and checklist items.
- Supported checklist item types in MVP:
  - yes/no
  - text
  - numeric
  - select
- Checklist items may define:
  - required answer
  - comment required on failure
  - photo required on failure
  - minimum allowed value
  - maximum allowed value
  - critical flag

### Preventive Execution
- A technician executes a preventive template for an inventory item.
- Preventive execution statuses in MVP:
  - Draft
  - Submitted
  - Approved
  - Rejected
  - ReworkRequested
- A submitted preventive cannot be silently changed without explicit workflow support.
- Required answers must be present before submission.
- When a checklist item fails and policy requires justification, the justification must be provided.

### Preventive Validation
- Only authorized validators can validate submitted preventives.
- Validators can:
  - approve
  - reject
  - request rework
- Validation actions must store:
  - validator user
  - timestamp
  - comment or reason when required
- Invalid state transitions must be blocked.

---

## Backend Design Rules

### Solution Structure
Use a clear multi-project solution structure similar to:

- `InfraOps.Api`
- `InfraOps.Application`
- `InfraOps.Domain`
- `InfraOps.Infrastructure`
- `InfraOps.SharedKernel` only if truly needed
- `InfraOps.Domain.Tests`
- `InfraOps.Application.Tests`
- `InfraOps.Infrastructure.Tests`
- `InfraOps.Api.Tests`

### Feature Organization
Prefer organizing Application by feature or bounded context, for example:

- Identity
- Users
- Roles
- EntityTypes
- Inventory
- PreventiveTemplates
- PreventiveExecutions
- PreventiveValidations
- Sites
- Regions

Avoid dumping all commands or all validators into giant global folders with no feature grouping.

### API Rules
- Use RESTful endpoints.
- Return appropriate status codes.
- Use consistent error response shape.
- Validate input at the API boundary and also at application/domain levels where required.
- Never place business rules directly in controllers.
- Never access DbContext directly from controllers.

### Persistence Rules
- Use EF Core in Infrastructure only.
- Keep domain entities persistence-ignorant.
- Prefer explicit configuration via Fluent API.
- Use migrations.
- Include audit fields where relevant.
- Keep provider-specific concerns contained in Infrastructure.
- For local development, assume PostgreSQL unless instructed otherwise.

### Error Handling
- Use consistent exception handling middleware.
- Convert domain/application exceptions into meaningful API responses.
- Do not leak internal stack traces to clients.

### Authorization
- Use permission-based authorization in addition to roles where appropriate.
- Enforce authorization in application workflows or endpoint policies, not only in frontend UI.

---

## Frontend Design Rules

### Frontend Architecture
Organize the React application into clear modules:

- auth
- dashboard
- users
- roles
- entity-types
- inventory
- preventive-templates
- preventive-executions
- preventive-validations
- shared

### Component Guidelines
- Use functional components only.
- Use hooks for behavior composition.
- Keep presentation components separate from data-fetching concerns where practical.
- Prefer small, composable components.
- Avoid giant page components with embedded business logic.

### Data Fetching
- Use TanStack Query for server state.
- Centralize API client logic.
- Handle loading, empty, and error states explicitly.
- Do not scatter raw fetch logic across components.

### Forms
- Use React Hook Form with Zod validation.
- Build reusable field renderer components for dynamic entity fields and preventive checklist items.
- Keep form schema logic explicit and testable.

### Routing
- Use protected routes for authenticated areas.
- Enforce permission-aware UI rendering, but never rely on frontend-only authorization.

### UI Expectations
- The admin can configure entity types and preventive templates through structured forms.
- Dynamic form rendering is a key feature of the MVP.
- Do not implement drag-and-drop builders in the first MVP unless explicitly requested.

### Frontend Testing
Test key flows and components:
- login flow
- protected routing behavior
- dynamic form rendering
- preventive submission form behavior
- validation action behavior

---

## Coding Standards

### General
- Prefer explicit, readable code over clever code.
- Prefer composition over inheritance.
- Keep methods focused and reasonably small.
- Keep classes cohesive.
- Avoid duplication of business logic across layers.
- Remove dead code promptly.
- Do not leave TODO comments without context and owner intent.

### Naming
- Use clear, business-oriented names.
- Avoid vague names such as `Helper`, `Manager`, `Util`, `Misc`, `CommonService`.
- Name classes by responsibility:
  - `CreateEntityTypeCommand`
  - `SubmitPreventiveExecutionHandler`
  - `InventoryItem`
  - `PreventiveTemplate`
  - `PermissionRequirement`

### Async
- Use async/await properly.
- Do not block async flows with `.Result` or `.Wait()`.

### Logging
- Log meaningful operational events.
- Do not use logs as a substitute for proper error handling.
- Never log secrets or tokens.

---

## Anti-Patterns to Avoid

Do not generate code that does any of the following:

- puts business logic in controllers
- puts business logic in EF Core entities purely for persistence convenience
- uses DbContext directly in Presentation layer
- creates large god services with many unrelated responsibilities
- duplicates validation rules in multiple layers without purpose
- hardcodes entity-specific screens when metadata-driven rendering is intended
- relies on frontend-only authorization
- mixes infrastructure concerns into Domain or Application
- creates generic abstractions too early without demonstrated need
- uses jQuery or legacy DOM-manipulation patterns in React
- introduces drag-and-drop configuration UIs in MVP unless explicitly requested
- reproduces proprietary names, exact legacy table designs, or confidential structures

---

## Seed Data Guidance
For demo and portfolio purposes, prefer generic sample entity types such as:

- UPS
- Generator
- HVAC

Use these to demonstrate:
- entity definition
- inventory registration
- preventive template assignment
- preventive execution
- preventive validation

Do not seed proprietary customer data.

---

## Definition of Done

A task is only done when all of the following are true where applicable:

- business behavior is implemented
- tests are added and passing
- architecture boundaries are respected
- naming is clear
- authorization is considered
- error handling is considered
- UI states are handled
- no obvious duplication is introduced

---

## How Agents Should Work on This Repository

### Local Runtime Assumptions
- Prefer the Docker development stack for local runtime verification.
- The standard local stack is:
  - frontend
  - api
  - postgres
- Keep frontend and backend development workflows compatible with hot reload inside containers.
- When changing local environment behavior, update Docker assets, environment examples, and README together.

### Database Inspection Assumptions
- Prefer the local MCP database server for schema inspection when it is available.
- MCP database access should remain schema-focused and read-only by default.
- Do not introduce unrestricted database execution tooling for local development unless explicitly requested and justified.

When implementing features:

1. identify the bounded context or feature area
2. define or refine the domain rule first
3. write or update tests before business implementation
4. implement domain and application behavior
5. implement infrastructure details
6. expose via API
7. implement frontend integration
8. verify end-to-end behavior
9. keep changes small, coherent, and reviewable

When unsure:
- favor simpler designs
- preserve Clean Architecture boundaries
- preserve TDD discipline
- ask whether the change belongs to the fixed core or configurable layer
- prefer extensibility where the product explicitly requires configurability

---

## Initial MVP Priorities
Build in this order unless instructed otherwise:

1. solution scaffolding
2. authentication and authorization
3. user and role management
4. entity type management
5. inventory management
6. preventive template management
7. preventive execution
8. preventive validation

---

## Final Expectation
InfraOps is intended to be a portfolio-grade enterprise application. Every implementation decision should optimize for:

- maintainability
- testability
- clarity
- extensibility
- realistic business value
- professional architecture
