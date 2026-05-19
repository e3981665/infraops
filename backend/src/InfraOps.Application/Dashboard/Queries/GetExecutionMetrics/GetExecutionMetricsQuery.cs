using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Dashboard.Dtos;

namespace InfraOps.Application.Dashboard.Queries.GetExecutionMetrics;

public sealed record GetExecutionMetricsQuery(
    Guid? RegionId,
    Guid? SiteId,
    Guid? EntityTypeId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc)
    : IQuery<ExecutionMetricsDto>, IDashboardQuery;
