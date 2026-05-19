using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.PreventiveTemplates.Dtos;
using InfraOps.Application.PreventiveTemplates.Support;

namespace InfraOps.Application.PreventiveTemplates.Queries.GetPreventiveTemplateFormMetadata;

public sealed class GetPreventiveTemplateFormMetadataQueryHandler : IQueryHandler<GetPreventiveTemplateFormMetadataQuery, PreventiveTemplateFormMetadataDto>
{
    private readonly IEntityTypeRepository _entityTypeRepository;

    public GetPreventiveTemplateFormMetadataQueryHandler(IEntityTypeRepository entityTypeRepository)
    {
        _entityTypeRepository = entityTypeRepository;
    }

    public async Task<PreventiveTemplateFormMetadataDto> Handle(
        GetPreventiveTemplateFormMetadataQuery query,
        CancellationToken cancellationToken)
    {
        var entityTypes = await _entityTypeRepository.ListActiveAsync(cancellationToken);

        return PreventiveTemplateMappings.ToFormMetadataDto(entityTypes);
    }
}
