using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Dashboard.Dtos;

namespace InfraOps.Application.Dashboard.Queries.GetDashboardCharts;

public sealed record GetDashboardChartsQuery(
    Guid? RegionId,
    Guid? SiteId,
    Guid? EntityTypeId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc)
    : IQuery<DashboardChartsDto>, IDashboardQuery;
