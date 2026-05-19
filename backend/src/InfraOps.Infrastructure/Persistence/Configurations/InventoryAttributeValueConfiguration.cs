using InfraOps.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class InventoryAttributeValueConfiguration : IEntityTypeConfiguration<InventoryAttributeValue>
{
    public void Configure(EntityTypeBuilder<InventoryAttributeValue> builder)
    {
        builder.ToTable("inventory_attribute_values");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.InventoryItemId)
            .IsRequired();

        builder.Property(x => x.EntityFieldDefinitionId)
            .IsRequired();

        builder.Property(x => x.FieldKey)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(5000)
            .IsRequired();

        builder.HasIndex(x => new { x.InventoryItemId, x.FieldKey })
            .IsUnique();

        builder.HasOne(x => x.FieldDefinition)
            .WithMany()
            .HasForeignKey(x => x.EntityFieldDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
