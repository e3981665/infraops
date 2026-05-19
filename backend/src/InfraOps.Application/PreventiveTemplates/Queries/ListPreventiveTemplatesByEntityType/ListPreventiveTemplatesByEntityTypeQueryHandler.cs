using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Application.PreventiveTemplates.Dtos;
using InfraOps.Application.PreventiveTemplates.Support;

namespace InfraOps.Application.PreventiveTemplates.Queries.ListPreventiveTemplatesByEntityType;

public sealed class ListPreventiveTemplatesByEntityTypeQueryHandler : IQueryHandler<ListPreventiveTemplatesByEntityTypeQuery, IReadOnlyCollection<PreventiveTemplateDetailsDto>>
{
    private readonly IPreventiveTemplateRepository _preventiveTemplateRepository;

    public ListPreventiveTemplatesByEntityTypeQueryHandler(IPreventiveTemplateRepository preventiveTemplateRepository)
    {
        _preventiveTemplateRepository = preventiveTemplateRepository;
    }

    public async Task<IReadOnlyCollection<PreventiveTemplateDetailsDto>> Handle(
        ListPreventiveTemplatesByEntityTypeQuery query,
        CancellationToken cancellationToken)
    {
        var preventiveTemplates = await _preventiveTemplateRepository.ListByEntityTypeAsync(
            query.EntityTypeId,
            true,
            cancellationToken);

        return preventiveTemplates
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => PreventiveTemplateMappings.ToDetailsDto(x, activeOnly: true))
            .ToArray();
    }
}
