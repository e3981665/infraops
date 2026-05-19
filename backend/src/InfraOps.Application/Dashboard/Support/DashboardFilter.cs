namespace InfraOps.Application.Dashboard.Support;

public sealed record DashboardFilter(
    Guid? RegionId,
    Guid? SiteId,
    Guid? EntityTypeId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc);
