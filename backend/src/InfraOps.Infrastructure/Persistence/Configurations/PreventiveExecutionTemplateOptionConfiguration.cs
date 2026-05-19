using InfraOps.Domain.PreventiveExecutions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class PreventiveExecutionTemplateOptionConfiguration
    : IEntityTypeConfiguration<PreventiveExecutionTemplateOption>
{
    public void Configure(EntityTypeBuilder<PreventiveExecutionTemplateOption> builder)
    {
        builder.ToTable("preventive_execution_template_options");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PreventiveExecutionTemplateItemId)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Label)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.HasIndex(x => new { x.PreventiveExecutionTemplateItemId, x.Value })
            .IsUnique();

        builder.HasIndex(x => new { x.PreventiveExecutionTemplateItemId, x.DisplayOrder })
            .IsUnique();
    }
}
