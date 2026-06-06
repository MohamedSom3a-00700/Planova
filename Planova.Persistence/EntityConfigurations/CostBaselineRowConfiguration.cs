using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using CostBaselineRowEntity = Planova.Cost.Domain.Entities.CostBaselineRow;

public class CostBaselineRowConfiguration : IEntityTypeConfiguration<CostBaselineRowEntity>
{
    public void Configure(EntityTypeBuilder<CostBaselineRowEntity> builder)
    {
        builder.ToTable("CostBaselineRows");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.PlannedCost).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.BudgetAtCompletion).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(e => new { e.BaselineId, e.ActivityId });
    }
}
