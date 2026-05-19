using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveTemplates.Dtos;

namespace InfraOps.Application.PreventiveTemplates.Queries.ListPreventiveTemplates;

public sealed record ListPreventiveTemplatesQuery(
    Guid? EntityTypeId,
    bool? IsActive,
    string? Search) : IQuery<IReadOnlyCollection<PreventiveTemplateSummaryDto>>;
