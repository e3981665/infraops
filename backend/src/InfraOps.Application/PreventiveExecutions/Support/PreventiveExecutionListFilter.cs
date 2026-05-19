using InfraOps.Domain.PreventiveExecutions.Enums;

namespace InfraOps.Application.PreventiveExecutions.Support;

public sealed record PreventiveExecutionListFilter(
    PreventiveExecutionStatus? Status,
    Guid? EntityTypeId,
    Guid? InventoryItemId,
    Guid? SiteId,
    Guid? RegionId,
    Guid? CreatedBy,
    Guid? SubmittedBy,
    DateTimeOffset? StartedFromUtc,
    DateTimeOffset? StartedToUtc,
    DateTimeOffset? SubmittedFromUtc,
    DateTimeOffset? SubmittedToUtc,
    string? Search);
