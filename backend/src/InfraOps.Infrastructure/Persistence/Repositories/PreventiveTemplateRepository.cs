using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Application.PreventiveTemplates.Support;
using InfraOps.Domain.PreventiveTemplates.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraOps.Infrastructure.Persistence.Repositories;

public sealed class PreventiveTemplateRepository : IPreventiveTemplateRepository
{
    private readonly InfraOpsDbContext _dbContext;

    public PreventiveTemplateRepository(InfraOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PreventiveTemplate preventiveTemplate, CancellationToken cancellationToken)
    {
        await _dbContext.PreventiveTemplates.AddAsync(preventiveTemplate, cancellationToken);
    }

    public Task<PreventiveTemplate?> GetByIdAsync(Guid preventiveTemplateId, CancellationToken cancellationToken)
    {
        return _dbContext.PreventiveTemplates
            .Include(x => x.EntityType)
            .Include(x => x.Sections)
            .ThenInclude(x => x.ChecklistItems)
            .ThenInclude(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == preventiveTemplateId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PreventiveTemplate>> ListAsync(
        PreventiveTemplateListFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.PreventiveTemplates
            .Include(x => x.EntityType)
            .Include(x => x.Sections)
            .ThenInclude(x => x.ChecklistItems)
            .AsQueryable();

        if (filter.EntityTypeId.HasValue)
        {
            query = query.Where(x => x.EntityTypeId == filter.EntityTypeId.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var normalizedSearch = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(normalizedSearch) ||
                x.Code.ToLower().Contains(normalizedSearch));
        }

        return await query
            .OrderBy(x => x.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PreventiveTemplate>> ListByEntityTypeAsync(
        Guid entityTypeId,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.PreventiveTemplates
            .Include(x => x.EntityType)
            .Include(x => x.Sections)
            .ThenInclude(x => x.ChecklistItems)
            .ThenInclude(x => x.Options)
            .Where(x => x.EntityTypeId == entityTypeId)
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> IsCodeInUseAsync(
        string normalizedCode,
        Guid? excludedPreventiveTemplateId,
        CancellationToken cancellationToken)
    {
        return _dbContext.PreventiveTemplates.AnyAsync(
            x => x.Code == normalizedCode
                && (!excludedPreventiveTemplateId.HasValue || x.Id != excludedPreventiveTemplateId.Value),
            cancellationToken);
    }
}
