using InfraOps.Application.Dashboard.Dtos;
using InfraOps.Application.Dashboard.Support;

namespace InfraOps.Application.Dashboard.Abstractions;

public interface IDashboardMetricsRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync(DashboardFilter filter, DateTimeOffset nowUtc, CancellationToken cancellationToken);

    Task<ExecutionMetricsDto> GetExecutionMetricsAsync(DashboardFilter filter, CancellationToken cancellationToken);

    Task<ValidationMetricsDto> GetValidationMetricsAsync(DashboardFilter filter, CancellationToken cancellationToken);

    Task<InventoryMetricsDto> GetInventoryMetricsAsync(DashboardFilter filter, CancellationToken cancellationToken);

    Task<DashboardChartsDto> GetChartsAsync(DashboardFilter filter, CancellationToken cancellationToken);
}
