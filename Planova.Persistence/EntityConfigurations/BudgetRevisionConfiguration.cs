using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using BudgetRevisionEntity = Planova.Cost.Domain.Entities.BudgetRevision;

public class BudgetRevisionConfiguration : IEntityTypeConfiguration<BudgetRevisionEntity>
{
    public void Configure(EntityTypeBuilder<BudgetRevisionEntity> builder)
    {
        builder.ToTable("BudgetRevisions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.RevisionType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Reason).HasMaxLength(500);
        builder.Property(e => e.ApprovedBy).HasMaxLength(100);
        builder.Property(e => e.CreatedBy).HasMaxLength(100);

        builder.HasIndex(e => new { e.BudgetId, e.RevisionNumber }).IsUnique();
    }
}
