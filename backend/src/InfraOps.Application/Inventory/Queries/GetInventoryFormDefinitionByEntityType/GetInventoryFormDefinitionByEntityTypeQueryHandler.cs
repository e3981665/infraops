using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.Inventory.Dtos;
using InfraOps.Application.Inventory.Support;

namespace InfraOps.Application.Inventory.Queries.GetInventoryFormDefinitionByEntityType;

public sealed class GetInventoryFormDefinitionByEntityTypeQueryHandler
    : IQueryHandler<GetInventoryFormDefinitionByEntityTypeQuery, InventoryFormDefinitionDto>
{
    private readonly IEntityTypeRepository _entityTypeRepository;

    public GetInventoryFormDefinitionByEntityTypeQueryHandler(IEntityTypeRepository entityTypeRepository)
    {
        _entityTypeRepository = entityTypeRepository;
    }

    public async Task<InventoryFormDefinitionDto> Handle(
        GetInventoryFormDefinitionByEntityTypeQuery query,
        CancellationToken cancellationToken)
    {
        var entityType = await _entityTypeRepository.GetByIdAsync(query.EntityTypeId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Entity type was not found.");

        if (!entityType.IsActive)
        {
            throw new ApplicationNotFoundException("Entity type form definition is not available for inactive entity types.");
        }

        return InventoryMappings.ToFormDefinitionDto(entityType);
    }
}
