using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Application.PreventiveTemplates.Dtos;
using InfraOps.Application.PreventiveTemplates.Support;

namespace InfraOps.Application.PreventiveTemplates.Queries.GetPreventiveTemplateById;

public sealed class GetPreventiveTemplateByIdQueryHandler : IQueryHandler<GetPreventiveTemplateByIdQuery, PreventiveTemplateDetailsDto>
{
    private readonly IPreventiveTemplateRepository _preventiveTemplateRepository;

    public GetPreventiveTemplateByIdQueryHandler(IPreventiveTemplateRepository preventiveTemplateRepository)
    {
        _preventiveTemplateRepository = preventiveTemplateRepository;
    }

    public async Task<PreventiveTemplateDetailsDto> Handle(
        GetPreventiveTemplateByIdQuery query,
        CancellationToken cancellationToken)
    {
        var preventiveTemplate = await _preventiveTemplateRepository.GetByIdAsync(query.PreventiveTemplateId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Preventive template was not found.");

        return PreventiveTemplateMappings.ToDetailsDto(preventiveTemplate);
    }
}
