# Preventive Execution And Validation Flow

This document describes the operational workflow from inventory item selection through validation.

## Execution Flow

```mermaid
sequenceDiagram
    actor Technician
    participant UI as React UI
    participant API as ASP.NET API
    participant App as Application Handler
    participant Domain as PreventiveExecution Aggregate
    participant DB as PostgreSQL

    Technician->>UI: Select inventory item
    UI->>API: GET /api/preventive-executions/form-definition/{inventoryItemId}
    API->>App: Resolve inventory item and active template
    App->>DB: Load inventory + active template
    API-->>UI: Dynamic checklist definition

    Technician->>UI: Start execution
    UI->>API: POST /api/preventive-executions/start
    App->>Domain: CreateDraft(inventory, template)
    Domain-->>App: Execution with template snapshot
    App->>DB: Persist draft

    Technician->>UI: Save draft or submit
    UI->>API: PUT draft or POST submit
    App->>Domain: UpdateDraft or Submit
    Domain->>Domain: Validate answers against snapshot
    App->>DB: Persist status and answers
```

## Validation Flow

```mermaid
stateDiagram-v2
    [*] --> Draft
    Draft --> Submitted: technician submits valid answers
    Submitted --> Approved: validator approves
    Submitted --> Rejected: validator rejects with reason
    Submitted --> ReworkRequested: validator requests rework with reason
```

Invalid transitions are blocked by the aggregate:

- Draft cannot be approved, rejected, or sent to rework.
- Approved cannot be approved again.
- Rejected cannot be approved without a future explicit rework flow.
- ReworkRequested cannot be approved without a future resubmission flow.

## Audit Visibility

Execution detail and validation review screens expose:

- created by and created timestamp
- updated by and updated timestamp
- submitted by and submitted timestamp
- validation history with validator, action, timestamp, and comment or reason

## Dashboard Reuse

The dashboard consumes the same persisted operational facts:

- inventory item counts by active state and entity type
- preventive execution counts by status and month
- validation outcome counts by status
- submitted executions pending validator review

No dashboard endpoint mutates workflow state.
