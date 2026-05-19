using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Dashboard.Dtos;

namespace InfraOps.Application.Dashboard.Queries.GetValidationMetrics;

public sealed record GetValidationMetricsQuery(
    Guid? RegionId,
    Guid? SiteId,
    Guid? EntityTypeId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc)
    : IQuery<ValidationMetricsDto>, IDashboardQuery;
