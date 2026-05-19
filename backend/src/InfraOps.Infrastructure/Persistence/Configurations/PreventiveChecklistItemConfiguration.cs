using InfraOps.Domain.PreventiveTemplates.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class PreventiveChecklistItemConfiguration : IEntityTypeConfiguration<PreventiveChecklistItem>
{
    public void Configure(EntityTypeBuilder<PreventiveChecklistItem> builder)
    {
        builder.ToTable("preventive_checklist_items");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PreventiveTemplateSectionId)
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

        builder.Property(x => x.IsActive)
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

        builder.HasIndex(x => new { x.PreventiveTemplateSectionId, x.ItemKey })
            .IsUnique();

        builder.HasIndex(x => new { x.PreventiveTemplateSectionId, x.DisplayOrder })
            .IsUnique();

        builder.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(x => x.PreventiveChecklistItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Options)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
