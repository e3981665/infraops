using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Domain.Identity.Entities;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.Locations.Entities;
using InfraOps.Domain.PreventiveExecutions.Entities;
using InfraOps.Domain.PreventiveTemplates.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraOps.Infrastructure.Persistence;

public sealed class InfraOpsDbContext : DbContext, IUnitOfWork
{
    public InfraOpsDbContext(DbContextOptions<InfraOpsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<EntityType> EntityTypes => Set<EntityType>();

    public DbSet<EntityFieldDefinition> EntityFieldDefinitions => Set<EntityFieldDefinition>();

    public DbSet<EntityFieldOption> EntityFieldOptions => Set<EntityFieldOption>();

    public DbSet<Region> Regions => Set<Region>();

    public DbSet<Site> Sites => Set<Site>();

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<InventoryAttributeValue> InventoryAttributeValues => Set<InventoryAttributeValue>();

    public DbSet<PreventiveTemplate> PreventiveTemplates => Set<PreventiveTemplate>();

    public DbSet<PreventiveTemplateSection> PreventiveTemplateSections => Set<PreventiveTemplateSection>();

    public DbSet<PreventiveChecklistItem> PreventiveChecklistItems => Set<PreventiveChecklistItem>();

    public DbSet<PreventiveChecklistOption> PreventiveChecklistOptions => Set<PreventiveChecklistOption>();

    public DbSet<PreventiveExecution> PreventiveExecutions => Set<PreventiveExecution>();

    public DbSet<PreventiveExecutionTemplateSection> PreventiveExecutionTemplateSections => Set<PreventiveExecutionTemplateSection>();

    public DbSet<PreventiveExecutionTemplateItem> PreventiveExecutionTemplateItems => Set<PreventiveExecutionTemplateItem>();

    public DbSet<PreventiveExecutionTemplateOption> PreventiveExecutionTemplateOptions => Set<PreventiveExecutionTemplateOption>();

    public DbSet<PreventiveExecutionAnswer> PreventiveExecutionAnswers => Set<PreventiveExecutionAnswer>();

    public DbSet<PreventiveValidationRecord> PreventiveValidationRecords => Set<PreventiveValidationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfraOpsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await base.SaveChangesAsync(cancellationToken);
    }
}
