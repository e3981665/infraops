using InfraOps.Application.Locations.Abstractions;
using InfraOps.Domain.Locations.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraOps.Infrastructure.Persistence.Repositories;

public sealed class LocationLookupRepository : ILocationLookupRepository
{
    private readonly InfraOpsDbContext _dbContext;

    public LocationLookupRepository(InfraOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Region?> GetRegionByIdAsync(Guid regionId, CancellationToken cancellationToken)
    {
        return _dbContext.Regions.SingleOrDefaultAsync(x => x.Id == regionId, cancellationToken);
    }

    public Task<Site?> GetSiteByIdAsync(Guid siteId, CancellationToken cancellationToken)
    {
        return _dbContext.Sites
            .Include(x => x.Region)
            .SingleOrDefaultAsync(x => x.Id == siteId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Region>> ListActiveRegionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Regions
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Site>> ListActiveSitesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Sites
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToArrayAsync(cancellationToken);
    }
}
