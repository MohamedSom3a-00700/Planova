using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.ScheduleComparison.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ScheduleSnapshotConfiguration : IEntityTypeConfiguration<ScheduleSnapshot>
{
    public void Configure(EntityTypeBuilder<ScheduleSnapshot> builder)
    {
        builder.ToTable("ScheduleSnapshots");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Label).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SnapshotData).IsRequired();

        builder.HasIndex(e => e.ProjectId).HasDatabaseName("IX_ScheduleSnapshots_ProjectId");
    }
}
