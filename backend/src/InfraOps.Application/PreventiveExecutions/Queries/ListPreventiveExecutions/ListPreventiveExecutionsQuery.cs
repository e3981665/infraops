using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Dtos;

namespace InfraOps.Application.PreventiveExecutions.Queries.ListPreventiveExecutions;

public sealed record ListPreventiveExecutionsQuery(
    string? Status,
    Guid? EntityTypeId,
    Guid? InventoryItemId,
    Guid? SiteId,
    Guid? RegionId,
    bool CreatedByCurrentUser,
    DateTimeOffset? StartedFromUtc,
    DateTimeOffset? StartedToUtc,
    string? Search,
    Guid? SubmittedBy = null,
    DateTimeOffset? SubmittedFromUtc = null,
    DateTimeOffset? SubmittedToUtc = null)
    : IQuery<IReadOnlyCollection<PreventiveExecutionSummaryDto>>;
