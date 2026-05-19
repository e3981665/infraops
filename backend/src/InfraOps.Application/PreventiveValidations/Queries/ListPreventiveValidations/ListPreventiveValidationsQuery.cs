using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Dtos;

namespace InfraOps.Application.PreventiveValidations.Queries.ListPreventiveValidations;

public sealed record ListPreventiveValidationsQuery(
    string? Status,
    Guid? EntityTypeId,
    Guid? InventoryItemId,
    Guid? SiteId,
    Guid? RegionId,
    Guid? SubmittedBy,
    DateTimeOffset? SubmittedFromUtc,
    DateTimeOffset? SubmittedToUtc,
    string? Search)
    : IQuery<IReadOnlyCollection<PreventiveExecutionSummaryDto>>;
