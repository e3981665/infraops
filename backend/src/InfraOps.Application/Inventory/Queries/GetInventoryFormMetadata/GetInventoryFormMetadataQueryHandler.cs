using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.Inventory.Dtos;
using InfraOps.Application.Inventory.Support;
using InfraOps.Application.Locations.Abstractions;

namespace InfraOps.Application.Inventory.Queries.GetInventoryFormMetadata;

public sealed class GetInventoryFormMetadataQueryHandler : IQueryHandler<GetInventoryFormMetadataQuery, InventoryFormMetadataDto>
{
    private readonly IEntityTypeRepository _entityTypeRepository;
    private readonly ILocationLookupRepository _locationLookupRepository;

    public GetInventoryFormMetadataQueryHandler(
        IEntityTypeRepository entityTypeRepository,
        ILocationLookupRepository locationLookupRepository)
    {
        _entityTypeRepository = entityTypeRepository;
        _locationLookupRepository = locationLookupRepository;
    }

    public async Task<InventoryFormMetadataDto> Handle(
        GetInventoryFormMetadataQuery query,
        CancellationToken cancellationToken)
    {
        var entityTypes = await _entityTypeRepository.ListActiveAsync(cancellationToken);
        var regions = await _locationLookupRepository.ListActiveRegionsAsync(cancellationToken);
        var sites = await _locationLookupRepository.ListActiveSitesAsync(cancellationToken);

        return InventoryMappings.ToFormMetadataDto(entityTypes, regions, sites);
    }
}
