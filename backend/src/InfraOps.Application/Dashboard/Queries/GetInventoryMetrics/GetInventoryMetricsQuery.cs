using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Dashboard.Dtos;

namespace InfraOps.Application.Dashboard.Queries.GetInventoryMetrics;

public sealed record GetInventoryMetricsQuery(
    Guid? RegionId,
    Guid? SiteId,
    Guid? EntityTypeId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc)
    : IQuery<InventoryMetricsDto>, IDashboardQuery;
