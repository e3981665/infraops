using InfraOps.Domain.EntityTypes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class EntityFieldOptionConfiguration : IEntityTypeConfiguration<EntityFieldOption>
{
    public void Configure(EntityTypeBuilder<EntityFieldOption> builder)
    {
        builder.ToTable("entity_field_options");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.EntityFieldDefinitionId)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Label)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.HasIndex(x => new { x.EntityFieldDefinitionId, x.Value })
            .IsUnique();
    }
}
