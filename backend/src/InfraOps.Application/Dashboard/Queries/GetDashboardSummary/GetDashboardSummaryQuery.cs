using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Dashboard.Dtos;

namespace InfraOps.Application.Dashboard.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery(
    Guid? RegionId,
    Guid? SiteId,
    Guid? EntityTypeId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc)
    : IQuery<DashboardSummaryDto>, IDashboardQuery;
