using InfraOps.Domain.PreventiveTemplates.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class PreventiveTemplateSectionConfiguration : IEntityTypeConfiguration<PreventiveTemplateSection>
{
    public void Configure(EntityTypeBuilder<PreventiveTemplateSection> builder)
    {
        builder.ToTable("preventive_template_sections");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PreventiveTemplateId)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => new { x.PreventiveTemplateId, x.DisplayOrder })
            .IsUnique();

        builder.HasMany(x => x.ChecklistItems)
            .WithOne()
            .HasForeignKey(x => x.PreventiveTemplateSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.ChecklistItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
