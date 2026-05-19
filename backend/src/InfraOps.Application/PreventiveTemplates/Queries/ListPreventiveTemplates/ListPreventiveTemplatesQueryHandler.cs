using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Application.PreventiveTemplates.Dtos;
using InfraOps.Application.PreventiveTemplates.Support;

namespace InfraOps.Application.PreventiveTemplates.Queries.ListPreventiveTemplates;

public sealed class ListPreventiveTemplatesQueryHandler : IQueryHandler<ListPreventiveTemplatesQuery, IReadOnlyCollection<PreventiveTemplateSummaryDto>>
{
    private readonly IValidator<ListPreventiveTemplatesQuery> _validator;
    private readonly IPreventiveTemplateRepository _preventiveTemplateRepository;

    public ListPreventiveTemplatesQueryHandler(
        IValidator<ListPreventiveTemplatesQuery> validator,
        IPreventiveTemplateRepository preventiveTemplateRepository)
    {
        _validator = validator;
        _preventiveTemplateRepository = preventiveTemplateRepository;
    }

    public async Task<IReadOnlyCollection<PreventiveTemplateSummaryDto>> Handle(
        ListPreventiveTemplatesQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var preventiveTemplates = await _preventiveTemplateRepository.ListAsync(
            new PreventiveTemplateListFilter(query.EntityTypeId, query.IsActive, query.Search),
            cancellationToken);

        return preventiveTemplates
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(PreventiveTemplateMappings.ToSummaryDto)
            .ToArray();
    }
}
