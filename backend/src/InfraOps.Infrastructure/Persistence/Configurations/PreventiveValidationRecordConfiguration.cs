using InfraOps.Domain.PreventiveExecutions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class PreventiveValidationRecordConfiguration : IEntityTypeConfiguration<PreventiveValidationRecord>
{
    public void Configure(EntityTypeBuilder<PreventiveValidationRecord> builder)
    {
        builder.ToTable("preventive_validation_records");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PreventiveExecutionId)
            .IsRequired();

        builder.Property(x => x.ActionType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ValidatorUserId)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.PreventiveExecutionId);
        builder.HasIndex(x => x.ValidatorUserId);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
