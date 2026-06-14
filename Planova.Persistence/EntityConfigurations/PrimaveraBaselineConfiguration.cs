using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using PrimaveraBaseline = Planova.Primavera.Domain.Entities.PrimaveraBaseline;

public class PrimaveraBaselineConfiguration : IEntityTypeConfiguration<PrimaveraBaseline>
{
    public void Configure(EntityTypeBuilder<PrimaveraBaseline> builder)
    {
        builder.ToTable("PrimaveraBaselines");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.BaselineId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(500).IsRequired();
        builder.HasIndex(e => new { e.ProjectId, e.BaselineId, e.VersionNumber });
    }
}
