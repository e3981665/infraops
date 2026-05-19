using InfraOps.Domain.PreventiveTemplates.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class PreventiveChecklistOptionConfiguration : IEntityTypeConfiguration<PreventiveChecklistOption>
{
    public void Configure(EntityTypeBuilder<PreventiveChecklistOption> builder)
    {
        builder.ToTable("preventive_checklist_options");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PreventiveChecklistItemId)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Label)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.HasIndex(x => new { x.PreventiveChecklistItemId, x.Value })
            .IsUnique();

        builder.HasIndex(x => new { x.PreventiveChecklistItemId, x.DisplayOrder })
            .IsUnique();
    }
}
