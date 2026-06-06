using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using CostBaselineEntity = Planova.Cost.Domain.Entities.CostBaseline;

public class CostBaselineConfiguration : IEntityTypeConfiguration<CostBaselineEntity>
{
    public void Configure(EntityTypeBuilder<CostBaselineEntity> builder)
    {
        builder.ToTable("CostBaselines");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();

        builder.HasIndex(e => e.ProjectId)
               .IsUnique()
               .HasFilter("[IsActive] = 1");

        builder.HasMany(e => e.Rows)
               .WithOne(e => e.Baseline)
               .HasForeignKey(e => e.BaselineId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
