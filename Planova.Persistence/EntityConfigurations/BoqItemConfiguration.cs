using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

public class BoqItemConfiguration : IEntityTypeConfiguration<Planova.Boq.Domain.Entities.BoqItem>
{
    public void Configure(EntityTypeBuilder<Planova.Boq.Domain.Entities.BoqItem> builder)
    {
        builder.ToTable("BoqItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Quantity)
            .HasPrecision(18, 4);

        builder.Property(e => e.Rate)
            .HasPrecision(18, 4);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.ItemType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.CostCode)
            .HasMaxLength(100);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(e => e.BoqId)
            .HasDatabaseName("IX_BoqItems_BoqId");

        builder.HasIndex(e => e.ParentId)
            .HasDatabaseName("IX_BoqItems_ParentId");

        builder.HasIndex(e => new { e.BoqId, e.Code })
            .IsUnique()
            .HasDatabaseName("IX_BoqItems_BoqId_Code");

        builder.HasOne(e => e.Boq)
            .WithMany(b => b.Items)
            .HasForeignKey(e => e.BoqId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
