using InfraOps.Application.Dashboard.Abstractions;
using InfraOps.Application.Dashboard.Dtos;
using InfraOps.Application.Dashboard.Support;
using InfraOps.Domain.PreventiveExecutions.Enums;
using Microsoft.EntityFrameworkCore;

namespace InfraOps.Infrastructure.Persistence.Repositories;

public sealed class DashboardMetricsRepository : IDashboardMetricsRepository
{
    private readonly InfraOpsDbContext _dbContext;

    public DashboardMetricsRepository(InfraOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        DashboardFilter filter,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        var inventoryRows = await GetInventoryRowsAsync(filter, cancellationToken);
        var executionRows = await GetExecutionRowsAsync(filter, cancellationToken);
        var monthStart = new DateTimeOffset(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);

        return new DashboardSummaryDto(
            inventoryRows.Length,
            inventoryRows.Count(x => x.IsActive),
            executionRows.Count(x => x.CreatedAtUtc >= monthStart),
            executionRows.Count(x => x.Status == PreventiveExecutionStatus.Submitted),
            executionRows.Count(x => x.Status == PreventiveExecutionStatus.Approved),
            executionRows.Count(x => x.Status == PreventiveExecutionStatus.Rejected),
            executionRows.Count(x => x.Status == PreventiveExecutionStatus.ReworkRequested),
            await _dbContext.EntityTypes.AsNoTracking().CountAsync(x => x.IsActive, cancellationToken),
            await _dbContext.PreventiveTemplates.AsNoTracking().CountAsync(x => x.IsActive, cancellationToken));
    }

    public async Task<ExecutionMetricsDto> GetExecutionMetricsAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var rows = await GetExecutionRowsAsync(filter, cancellationToken);

        return new ExecutionMetricsDto(
            rows.Length,
            rows.Count(x => x.Status == PreventiveExecutionStatus.Draft),
            rows.Count(x => x.Status == PreventiveExecutionStatus.Submitted),
            rows.Count(x => x.Status == PreventiveExecutionStatus.Approved),
            rows.Count(x => x.Status == PreventiveExecutionStatus.Rejected),
            rows.Count(x => x.Status == PreventiveExecutionStatus.ReworkRequested),
            GetExecutionCountsByEntityType(rows));
    }

    public async Task<ValidationMetricsDto> GetValidationMetricsAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var rows = await GetExecutionRowsAsync(filter, cancellationToken);
        var submitted = rows.Count(x => x.Status == PreventiveExecutionStatus.Submitted);
        var approved = rows.Count(x => x.Status == PreventiveExecutionStatus.Approved);
        var rejected = rows.Count(x => x.Status == PreventiveExecutionStatus.Rejected);
        var reworkRequested = rows.Count(x => x.Status == PreventiveExecutionStatus.ReworkRequested);

        return new ValidationMetricsDto(
            submitted,
            approved,
            rejected,
            reworkRequested,
            [
                new DashboardMetricPointDto("Pending validation", submitted),
                new DashboardMetricPointDto("Approved", approved),
                new DashboardMetricPointDto("Rejected", rejected),
                new DashboardMetricPointDto("Rework requested", reworkRequested)
            ]);
    }

    public async Task<InventoryMetricsDto> GetInventoryMetricsAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var rows = await GetInventoryRowsAsync(filter, cancellationToken);

        return new InventoryMetricsDto(
            rows.Length,
            rows.Count(x => x.IsActive),
            GetInventoryCountsByEntityType(rows));
    }

    public async Task<DashboardChartsDto> GetChartsAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var executions = await GetExecutionRowsAsync(filter, cancellationToken);
        var inventoryRows = await GetInventoryRowsAsync(filter, cancellationToken);

        var executionsByMonth = executions
            .GroupBy(x => new DateTime(x.CreatedAtUtc.Year, x.CreatedAtUtc.Month, 1))
            .OrderBy(x => x.Key)
            .Select(x => new DashboardMetricPointDto(x.Key.ToString("yyyy-MM"), x.Count()))
            .ToArray();

        var validationResults = new[]
        {
            new DashboardMetricPointDto("Pending validation", executions.Count(x => x.Status == PreventiveExecutionStatus.Submitted)),
            new DashboardMetricPointDto("Approved", executions.Count(x => x.Status == PreventiveExecutionStatus.Approved)),
            new DashboardMetricPointDto("Rejected", executions.Count(x => x.Status == PreventiveExecutionStatus.Rejected)),
            new DashboardMetricPointDto("Rework requested", executions.Count(x => x.Status == PreventiveExecutionStatus.ReworkRequested))
        };

        return new DashboardChartsDto(
            executionsByMonth,
            validationResults,
            GetInventoryCountsByEntityType(inventoryRows),
            GetExecutionCountsByEntityType(executions));
    }

    private static IQueryable<Domain.Inventory.Entities.InventoryItem> ApplyInventoryFilter(
        IQueryable<Domain.Inventory.Entities.InventoryItem> query,
        DashboardFilter filter)
    {
        if (filter.RegionId.HasValue)
        {
            query = query.Where(x => x.RegionId == filter.RegionId.Value);
        }

        if (filter.SiteId.HasValue)
        {
            query = query.Where(x => x.SiteId == filter.SiteId.Value);
        }

        if (filter.EntityTypeId.HasValue)
        {
            query = query.Where(x => x.EntityTypeId == filter.EntityTypeId.Value);
        }

        return query;
    }

    private static IQueryable<Domain.PreventiveExecutions.Entities.PreventiveExecution> ApplyExecutionFilter(
        IQueryable<Domain.PreventiveExecutions.Entities.PreventiveExecution> query,
        DashboardFilter filter)
    {
        if (filter.RegionId.HasValue)
        {
            query = query.Where(x => x.RegionId == filter.RegionId.Value);
        }

        if (filter.SiteId.HasValue)
        {
            query = query.Where(x => x.SiteId == filter.SiteId.Value);
        }

        if (filter.EntityTypeId.HasValue)
        {
            query = query.Where(x => x.EntityTypeId == filter.EntityTypeId.Value);
        }

        return query;
    }

    private async Task<InventoryDashboardRow[]> GetInventoryRowsAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var rows = await ApplyInventoryFilter(_dbContext.InventoryItems.AsNoTracking(), filter)
            .Select(x => new InventoryDashboardRow(
                x.EntityType!.Name,
                x.IsActive,
                x.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return ApplyDateFilter(rows, filter).ToArray();
    }

    private async Task<ExecutionDashboardRow[]> GetExecutionRowsAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var rows = await ApplyExecutionFilter(_dbContext.PreventiveExecutions.AsNoTracking(), filter)
            .Select(x => new ExecutionDashboardRow(
                x.EntityTypeName,
                x.Status,
                x.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return ApplyDateFilter(rows, filter).ToArray();
    }

    private static IReadOnlyCollection<DashboardMetricPointDto> GetInventoryCountsByEntityType(
        IReadOnlyCollection<InventoryDashboardRow> rows)
    {
        return rows
            .GroupBy(x => x.EntityTypeName)
            .Select(x => new DashboardMetricPointDto(x.Key, x.Count()))
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Label)
            .ToArray();
    }

    private static IReadOnlyCollection<DashboardMetricPointDto> GetExecutionCountsByEntityType(
        IReadOnlyCollection<ExecutionDashboardRow> rows)
    {
        return rows
            .GroupBy(x => x.EntityTypeName)
            .Select(x => new DashboardMetricPointDto(x.Key, x.Count()))
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Label)
            .ToArray();
    }

    private static IEnumerable<TDashboardRow> ApplyDateFilter<TDashboardRow>(
        IEnumerable<TDashboardRow> rows,
        DashboardFilter filter)
        where TDashboardRow : IDashboardRow
    {
        if (filter.FromUtc.HasValue)
        {
            rows = rows.Where(x => x.CreatedAtUtc >= filter.FromUtc.Value);
        }

        if (filter.ToUtc.HasValue)
        {
            rows = rows.Where(x => x.CreatedAtUtc <= filter.ToUtc.Value);
        }

        return rows;
    }

    private interface IDashboardRow
    {
        DateTimeOffset CreatedAtUtc { get; }
    }

    private sealed record InventoryDashboardRow(
        string EntityTypeName,
        bool IsActive,
        DateTimeOffset CreatedAtUtc)
        : IDashboardRow;

    private sealed record ExecutionDashboardRow(
        string EntityTypeName,
        PreventiveExecutionStatus Status,
        DateTimeOffset CreatedAtUtc)
        : IDashboardRow;
}
