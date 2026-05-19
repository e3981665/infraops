using InfraOps.Domain.Locations.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraOps.Infrastructure.Persistence.Seeding;

public sealed class LocationDataSeeder
{
    private readonly InfraOpsDbContext _dbContext;

    public LocationDataSeeder(InfraOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await EnsureRegionsAsync(cancellationToken);
        await EnsureSitesAsync(cancellationToken);
    }

    private async Task EnsureRegionsAsync(CancellationToken cancellationToken)
    {
        var existingRegions = await _dbContext.Regions
            .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var (id, code, name) in LocationSeedData.Regions)
        {
            if (existingRegions.TryGetValue(code, out var region))
            {
                region.Activate();
                continue;
            }

            _dbContext.Regions.Add(new Region(id, code, name));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureSitesAsync(CancellationToken cancellationToken)
    {
        var existingSites = await _dbContext.Sites
            .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var (id, regionId, code, name) in LocationSeedData.Sites)
        {
            if (existingSites.TryGetValue(code, out var site))
            {
                site.Activate();
                continue;
            }

            _dbContext.Sites.Add(new Site(id, regionId, code, name));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
