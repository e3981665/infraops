using InfraOps.Application.Inventory.Abstractions;
using InfraOps.Application.Inventory.Support;
using InfraOps.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraOps.Infrastructure.Persistence.Repositories;

public sealed class InventoryItemRepository : IInventoryItemRepository
{
    private readonly InfraOpsDbContext _dbContext;

    public InventoryItemRepository(InfraOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken)
    {
        await _dbContext.InventoryItems.AddAsync(inventoryItem, cancellationToken);
    }

    public Task<InventoryItem?> GetByIdAsync(Guid inventoryItemId, CancellationToken cancellationToken)
    {
        return _dbContext.InventoryItems
            .Include(x => x.EntityType)
            .Include(x => x.Region)
            .Include(x => x.Site)
            .Include(x => x.AttributeValues)
            .ThenInclude(x => x.FieldDefinition)
            .ThenInclude(x => x!.Options)
            .SingleOrDefaultAsync(x => x.Id == inventoryItemId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryItem>> ListAsync(
        InventoryListFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.InventoryItems
            .Include(x => x.EntityType)
            .Include(x => x.Region)
            .Include(x => x.Site)
            .AsQueryable();

        if (filter.EntityTypeId.HasValue)
        {
            query = query.Where(x => x.EntityTypeId == filter.EntityTypeId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.SiteId.HasValue)
        {
            query = query.Where(x => x.SiteId == filter.SiteId.Value);
        }

        if (filter.RegionId.HasValue)
        {
            query = query.Where(x => x.RegionId == filter.RegionId.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var normalizedSearch = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.DisplayName.ToLower().Contains(normalizedSearch));
        }

        return await query
            .OrderBy(x => x.DisplayName)
            .ToArrayAsync(cancellationToken);
    }
}
