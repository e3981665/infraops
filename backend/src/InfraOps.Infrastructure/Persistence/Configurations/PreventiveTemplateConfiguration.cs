using InfraOps.Domain.PreventiveTemplates.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class PreventiveTemplateConfiguration : IEntityTypeConfiguration<PreventiveTemplate>
{
    public void Configure(EntityTypeBuilder<PreventiveTemplate> builder)
    {
        builder.ToTable("preventive_templates");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.EntityTypeId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasOne(x => x.EntityType)
            .WithMany()
            .HasForeignKey(x => x.EntityTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Sections)
            .WithOne()
            .HasForeignKey(x => x.PreventiveTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Sections)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
