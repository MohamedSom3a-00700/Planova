using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;

public class WbsConfiguration : IEntityTypeConfiguration<WbsEntity>
{
    public void Configure(EntityTypeBuilder<WbsEntity> builder)
    {
        builder.ToTable("Wbs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Revision)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.Source)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.TotalWeight)
            .HasPrecision(5, 2);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasIndex(e => e.ProjectId)
            .HasDatabaseName("IX_Wbs_ProjectId");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Wbs_Status");

        builder.HasMany(e => e.Items)
            .WithOne(e => e.Wbs)
            .HasForeignKey(e => e.WbsId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
