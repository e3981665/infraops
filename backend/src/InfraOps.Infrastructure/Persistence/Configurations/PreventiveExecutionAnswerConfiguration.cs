using InfraOps.Domain.PreventiveExecutions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraOps.Infrastructure.Persistence.Configurations;

public sealed class PreventiveExecutionAnswerConfiguration : IEntityTypeConfiguration<PreventiveExecutionAnswer>
{
    public void Configure(EntityTypeBuilder<PreventiveExecutionAnswer> builder)
    {
        builder.ToTable("preventive_execution_answers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PreventiveExecutionId)
            .IsRequired();

        builder.Property(x => x.ItemKey)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(2000);

        builder.Property(x => x.Comment)
            .HasMaxLength(2000);

        builder.HasIndex(x => new { x.PreventiveExecutionId, x.ItemKey })
            .IsUnique();
    }
}
