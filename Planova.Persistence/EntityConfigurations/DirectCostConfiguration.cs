using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using DirectCostEntity = Planova.Cost.Domain.Entities.DirectCost;

public class DirectCostConfiguration : IEntityTypeConfiguration<DirectCostEntity>
{
    public void Configure(EntityTypeBuilder<DirectCostEntity> builder)
    {
        builder.ToTable("DirectCosts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Category).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(e => e.CustomCategoryName).HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Quantity).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(e => e.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(e => e.UnitRate).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.Scope).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(e => new { e.ProjectId, e.Scope });
        builder.HasIndex(e => e.ActivityId);
    }
}
