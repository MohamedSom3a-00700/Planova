using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using WbsItemEntity = Planova.Wbs.Domain.Entities.WbsItem;

public class WbsItemConfiguration : IEntityTypeConfiguration<WbsItemEntity>
{
    public void Configure(EntityTypeBuilder<WbsItemEntity> builder)
    {
        builder.ToTable("WbsItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ShortCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.WbsLevel)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.Weight)
            .HasPrecision(5, 2);

        builder.Property(e => e.AssignedTo)
            .HasMaxLength(100);

        builder.Property(e => e.Deliverable)
            .HasMaxLength(500);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasOne(e => e.Wbs)
            .WithMany(e => e.Items)
            .HasForeignKey(e => e.WbsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.WbsId)
            .HasDatabaseName("IX_WbsItems_WbsId");

        builder.HasIndex(e => e.ParentId)
            .HasDatabaseName("IX_WbsItems_ParentId");

        builder.HasIndex(e => e.SourceBoqItemId)
            .HasDatabaseName("IX_WbsItems_SourceBoqItemId");
    }
}
