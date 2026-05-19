using InfraOps.Domain.Locations.Entities;

namespace InfraOps.Application.Locations.Abstractions;

public interface ILocationLookupRepository
{
    Task<Region?> GetRegionByIdAsync(Guid regionId, CancellationToken cancellationToken);

    Task<Site?> GetSiteByIdAsync(Guid siteId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Region>> ListActiveRegionsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Site>> ListActiveSitesAsync(CancellationToken cancellationToken);
}
