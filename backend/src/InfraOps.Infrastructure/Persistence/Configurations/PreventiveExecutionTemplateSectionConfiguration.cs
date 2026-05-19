using InfraOps.Domain.PreventiveExecutions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class PreventiveExecutionTemplateSectionConfiguration
    : IEntityTypeConfiguration<PreventiveExecutionTemplateSection>
{
    public void Configure(EntityTypeBuilder<PreventiveExecutionTemplateSection> builder)
    {
        builder.ToTable("preventive_execution_template_sections");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PreventiveExecutionId)
            .IsRequired();

        builder.Property(x => x.SourceTemplateSectionId)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.HasIndex(x => new { x.PreventiveExecutionId, x.DisplayOrder })
            .IsUnique();

        builder.HasMany(x => x.ChecklistItems)
            .WithOne()
            .HasForeignKey(x => x.PreventiveExecutionTemplateSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.ChecklistItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
