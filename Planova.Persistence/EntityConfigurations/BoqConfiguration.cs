using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

public class BoqConfiguration : IEntityTypeConfiguration<Planova.Boq.Domain.Entities.Boq>
{
    public void Configure(EntityTypeBuilder<Planova.Boq.Domain.Entities.Boq> builder)
    {
        builder.ToTable("Boqs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.ImportSource)
            .HasMaxLength(50);

        builder.Property(e => e.Version)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);

        builder.Property(e => e.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(e => e.ModifiedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasIndex(e => e.ProjectId)
            .HasDatabaseName("IX_Boqs_ProjectId");

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IX_Boqs_Name");

        builder.HasMany(e => e.Items)
            .WithOne(e => e.Boq)
            .HasForeignKey(e => e.BoqId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
