using InfraOps.Domain.EntityTypes.Entities;

namespace InfraOps.Application.EntityTypes.Abstractions;

public interface IEntityTypeRepository
{
    Task AddAsync(EntityType entityType, CancellationToken cancellationToken);

    Task<EntityType?> GetByIdAsync(Guid entityTypeId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<EntityType>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<EntityType>> ListActiveAsync(CancellationToken cancellationToken);

    Task<bool> IsCodeInUseAsync(
        string normalizedCode,
        Guid? excludedEntityTypeId,
        CancellationToken cancellationToken);
}
