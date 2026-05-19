namespace InfraOps.Application.Dashboard.Queries;

public interface IDashboardQuery
{
    Guid? RegionId { get; }

    Guid? SiteId { get; }

    Guid? EntityTypeId { get; }

    DateTimeOffset? FromUtc { get; }

    DateTimeOffset? ToUtc { get; }
}
