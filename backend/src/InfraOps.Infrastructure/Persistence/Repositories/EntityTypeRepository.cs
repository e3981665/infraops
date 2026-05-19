using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Domain.EntityTypes.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraOps.Infrastructure.Persistence.Repositories;

public sealed class EntityTypeRepository : IEntityTypeRepository
{
    private readonly InfraOpsDbContext _dbContext;

    public EntityTypeRepository(InfraOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EntityType entityType, CancellationToken cancellationToken)
    {
        await _dbContext.EntityTypes.AddAsync(entityType, cancellationToken);
    }

    public Task<EntityType?> GetByIdAsync(Guid entityTypeId, CancellationToken cancellationToken)
    {
        return _dbContext.EntityTypes
            .Include(x => x.FieldDefinitions)
            .ThenInclude(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == entityTypeId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<EntityType>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.EntityTypes
            .Include(x => x.FieldDefinitions)
            .OrderBy(x => x.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<EntityType>> ListActiveAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.EntityTypes
            .Include(x => x.FieldDefinitions)
            .ThenInclude(x => x.Options)
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> IsCodeInUseAsync(
        string normalizedCode,
        Guid? excludedEntityTypeId,
        CancellationToken cancellationToken)
    {
        return _dbContext.EntityTypes.AnyAsync(
            x => x.Code == normalizedCode && (!excludedEntityTypeId.HasValue || x.Id != excludedEntityTypeId.Value),
            cancellationToken);
    }
}
