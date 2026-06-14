using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using PrimaveraRepairAction = Planova.Primavera.Domain.Entities.PrimaveraRepairAction;

public class PrimaveraRepairActionConfiguration : IEntityTypeConfiguration<PrimaveraRepairAction>
{
    public void Configure(EntityTypeBuilder<PrimaveraRepairAction> builder)
    {
        builder.ToTable("PrimaveraRepairActions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.TargetEntityType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.AppliedBy).HasMaxLength(200).IsRequired();
        builder.HasIndex(e => e.ProjectId);
    }
}
