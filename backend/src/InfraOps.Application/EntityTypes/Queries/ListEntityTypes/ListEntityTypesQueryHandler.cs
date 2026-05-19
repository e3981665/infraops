using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.EntityTypes.Dtos;
using InfraOps.Application.EntityTypes.Support;

namespace InfraOps.Application.EntityTypes.Queries.ListEntityTypes;

public sealed class ListEntityTypesQueryHandler : IQueryHandler<ListEntityTypesQuery, IReadOnlyCollection<EntityTypeSummaryDto>>
{
    private readonly IEntityTypeRepository _entityTypeRepository;

    public ListEntityTypesQueryHandler(IEntityTypeRepository entityTypeRepository)
    {
        _entityTypeRepository = entityTypeRepository;
    }

    public async Task<IReadOnlyCollection<EntityTypeSummaryDto>> Handle(
        ListEntityTypesQuery query,
        CancellationToken cancellationToken)
    {
        var entityTypes = await _entityTypeRepository.ListAsync(cancellationToken);

        return entityTypes
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(EntityTypeMappings.ToSummaryDto)
            .ToArray();
    }
}
