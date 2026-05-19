using InfraOps.Domain.PreventiveExecutions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class PreventiveExecutionConfiguration : IEntityTypeConfiguration<PreventiveExecution>
{
    public void Configure(EntityTypeBuilder<PreventiveExecution> builder)
    {
        builder.ToTable("preventive_executions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.InventoryItemId)
            .IsRequired();

        builder.Property(x => x.PreventiveTemplateId)
            .IsRequired();

        builder.Property(x => x.PreventiveTemplateName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.PreventiveTemplateCode)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.EntityTypeId)
            .IsRequired();

        builder.Property(x => x.EntityTypeName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.EntityTypeCode)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.InventoryItemDisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.RegionId)
            .IsRequired();

        builder.Property(x => x.RegionName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.SiteId)
            .IsRequired();

        builder.Property(x => x.SiteName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.UpdatedBy)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.InventoryItemId);
        builder.HasIndex(x => x.PreventiveTemplateId);
        builder.HasIndex(x => x.EntityTypeId);
        builder.HasIndex(x => x.RegionId);
        builder.HasIndex(x => x.SiteId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedBy);
        builder.HasIndex(x => x.CreatedAtUtc);

        builder.HasOne(x => x.InventoryItem)
            .WithMany()
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PreventiveTemplate)
            .WithMany()
            .HasForeignKey(x => x.PreventiveTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TemplateSections)
            .WithOne()
            .HasForeignKey(x => x.PreventiveExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Answers)
            .WithOne()
            .HasForeignKey(x => x.PreventiveExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ValidationRecords)
            .WithOne()
            .HasForeignKey(x => x.PreventiveExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.TemplateSections)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Answers)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.ValidationRecords)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
