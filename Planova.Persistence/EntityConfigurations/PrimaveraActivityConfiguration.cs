using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using PrimaveraActivity = Planova.Primavera.Domain.Entities.PrimaveraActivity;

public class PrimaveraActivityConfiguration : IEntityTypeConfiguration<PrimaveraActivity>
{
    public void Configure(EntityTypeBuilder<PrimaveraActivity> builder)
    {
        builder.ToTable("PrimaveraActivities");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.TaskId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(50).IsRequired();
        builder.Property(e => e.SourceType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.TaskId);
    }
}
