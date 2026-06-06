using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using BudgetEntity = Planova.Cost.Domain.Entities.Budget;

public class BudgetConfiguration : IEntityTypeConfiguration<BudgetEntity>
{
    public void Configure(EntityTypeBuilder<BudgetEntity> builder)
    {
        builder.ToTable("Budgets");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.ResourceCostTotal).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.DirectCostTotal).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.ContingencyAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.ContingencyPercent).HasColumnType("decimal(5,2)");
        builder.Property(e => e.TotalBudget).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.ManualTotalBudget).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.ProjectId).IsUnique();

        builder.HasMany(e => e.Revisions)
               .WithOne(e => e.Budget)
               .HasForeignKey(e => e.BudgetId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
