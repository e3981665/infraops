using InfraOps.Domain.EntityTypes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class EntityFieldDefinitionConfiguration : IEntityTypeConfiguration<EntityFieldDefinition>
{
    public void Configure(EntityTypeBuilder<EntityFieldDefinition> builder)
    {
        builder.ToTable("entity_field_definitions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.EntityTypeId)
            .IsRequired();

        builder.Property(x => x.FieldKey)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.DisplayLabel)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.FieldType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsRequired)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.Placeholder)
            .HasMaxLength(200);

        builder.Property(x => x.HelpText)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.EntityTypeId, x.FieldKey })
            .IsUnique();

        builder.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(x => x.EntityFieldDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Options)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
