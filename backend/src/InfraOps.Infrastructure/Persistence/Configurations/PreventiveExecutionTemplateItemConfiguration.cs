using InfraOps.Domain.PreventiveExecutions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class PreventiveExecutionTemplateItemConfiguration
    : IEntityTypeConfiguration<PreventiveExecutionTemplateItem>
{
    public void Configure(EntityTypeBuilder<PreventiveExecutionTemplateItem> builder)
    {
        builder.ToTable("preventive_execution_template_items");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PreventiveExecutionTemplateSectionId)
            .IsRequired();

        builder.Property(x => x.SourceChecklistItemId)
            .IsRequired();

        builder.Property(x => x.ItemKey)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Label)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.ItemType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsRequired)
            .IsRequired();

        builder.Property(x => x.HelpText)
            .HasMaxLength(500);

        builder.Property(x => x.IsCritical)
            .IsRequired();

        builder.Property(x => x.RequiresCommentOnFailure)
            .IsRequired();

        builder.Property(x => x.RequiresPhotoOnFailure)
            .IsRequired();

        builder.Property(x => x.MinimumValue)
            .HasPrecision(18, 4);

        builder.Property(x => x.MaximumValue)
            .HasPrecision(18, 4);

        builder.HasIndex(x => new { x.PreventiveExecutionTemplateSectionId, x.ItemKey })
            .IsUnique();

        builder.HasIndex(x => new { x.PreventiveExecutionTemplateSectionId, x.DisplayOrder })
            .IsUnique();

        builder.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(x => x.PreventiveExecutionTemplateItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Options)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
