using InfraOps.Application.Dashboard.Support;

namespace InfraOps.Application.Dashboard.Queries;

public static class DashboardQueryMapping
{
    public static DashboardFilter ToFilter(IDashboardQuery query)
    {
        return new DashboardFilter(
            query.RegionId,
            query.SiteId,
            query.EntityTypeId,
            query.FromUtc,
            query.ToUtc);
    }
}
