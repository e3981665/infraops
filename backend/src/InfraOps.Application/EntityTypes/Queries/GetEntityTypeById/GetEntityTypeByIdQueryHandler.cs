using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.EntityTypes.Dtos;
using InfraOps.Application.EntityTypes.Support;

namespace InfraOps.Application.EntityTypes.Queries.GetEntityTypeById;

public sealed class GetEntityTypeByIdQueryHandler : IQueryHandler<GetEntityTypeByIdQuery, EntityTypeDetailsDto>
{
    private readonly IEntityTypeRepository _entityTypeRepository;

    public GetEntityTypeByIdQueryHandler(IEntityTypeRepository entityTypeRepository)
    {
        _entityTypeRepository = entityTypeRepository;
    }

    public async Task<EntityTypeDetailsDto> Handle(
        GetEntityTypeByIdQuery query,
        CancellationToken cancellationToken)
    {
        var entityType = await _entityTypeRepository.GetByIdAsync(query.EntityTypeId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Entity type was not found.");

        return EntityTypeMappings.ToDetailsDto(entityType);
    }
}
