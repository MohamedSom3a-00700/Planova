using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using ActualCostEntity = Planova.Cost.Domain.Entities.ActualCost;

public class ActualCostConfiguration : IEntityTypeConfiguration<ActualCostEntity>
{
    public void Configure(EntityTypeBuilder<ActualCostEntity> builder)
    {
        builder.ToTable("ActualCosts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.Source).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.ImportBatchId).HasMaxLength(50);

        builder.HasIndex(e => e.ActivityId).IsUnique();
        builder.HasIndex(e => e.ProjectId);
    }
}
