using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveExecutions.Support;
using InfraOps.Domain.PreventiveExecutions.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraOps.Infrastructure.Persistence.Repositories;

public sealed class PreventiveExecutionRepository : IPreventiveExecutionRepository
{
    private readonly InfraOpsDbContext _dbContext;

    public PreventiveExecutionRepository(InfraOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PreventiveExecution preventiveExecution, CancellationToken cancellationToken)
    {
        await _dbContext.PreventiveExecutions.AddAsync(preventiveExecution, cancellationToken);
    }

    public Task<PreventiveExecution?> GetByIdAsync(Guid preventiveExecutionId, CancellationToken cancellationToken)
    {
        return IncludeDetails(_dbContext.PreventiveExecutions)
            .SingleOrDefaultAsync(x => x.Id == preventiveExecutionId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PreventiveExecution>> ListAsync(
        PreventiveExecutionListFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.PreventiveExecutions.AsQueryable();

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.EntityTypeId.HasValue)
        {
            query = query.Where(x => x.EntityTypeId == filter.EntityTypeId.Value);
        }

        if (filter.InventoryItemId.HasValue)
        {
            query = query.Where(x => x.InventoryItemId == filter.InventoryItemId.Value);
        }

        if (filter.SiteId.HasValue)
        {
            query = query.Where(x => x.SiteId == filter.SiteId.Value);
        }

        if (filter.RegionId.HasValue)
        {
            query = query.Where(x => x.RegionId == filter.RegionId.Value);
        }

        if (filter.CreatedBy.HasValue)
        {
            query = query.Where(x => x.CreatedBy == filter.CreatedBy.Value);
        }

        if (filter.SubmittedBy.HasValue)
        {
            query = query.Where(x => x.SubmittedBy == filter.SubmittedBy.Value);
        }

        if (filter.StartedFromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= filter.StartedFromUtc.Value);
        }

        if (filter.StartedToUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= filter.StartedToUtc.Value);
        }

        if (filter.SubmittedFromUtc.HasValue)
        {
            query = query.Where(x => x.SubmittedAtUtc >= filter.SubmittedFromUtc.Value);
        }

        if (filter.SubmittedToUtc.HasValue)
        {
            query = query.Where(x => x.SubmittedAtUtc <= filter.SubmittedToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var normalizedSearch = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.InventoryItemDisplayName.ToLower().Contains(normalizedSearch)
                || x.PreventiveTemplateName.ToLower().Contains(normalizedSearch)
                || x.EntityTypeName.ToLower().Contains(normalizedSearch));
        }

        var executions = await query.ToArrayAsync(cancellationToken);

        return executions
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToArray();
    }

    private static IQueryable<PreventiveExecution> IncludeDetails(IQueryable<PreventiveExecution> query)
    {
        return query
            .Include(x => x.TemplateSections)
            .ThenInclude(x => x.ChecklistItems)
            .ThenInclude(x => x.Options)
            .Include(x => x.Answers)
            .Include(x => x.ValidationRecords);
    }
}
