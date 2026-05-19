using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveTemplates.Dtos;

namespace InfraOps.Application.PreventiveTemplates.Queries.ListPreventiveTemplatesByEntityType;

public sealed record ListPreventiveTemplatesByEntityTypeQuery(
    Guid EntityTypeId) : IQuery<IReadOnlyCollection<PreventiveTemplateDetailsDto>>;
