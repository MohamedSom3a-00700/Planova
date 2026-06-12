using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Reporting.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ReportInstanceConfiguration : IEntityTypeConfiguration<ReportInstance>
{
    public void Configure(EntityTypeBuilder<ReportInstance> builder)
    {
        builder.ToTable("ReportInstances");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.DataSnapshotJson).IsRequired();
        builder.Property(e => e.AiNarrative).HasMaxLength(5000);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.GeneratedBy).HasMaxLength(100);

        builder.Property(e => e.ReportType)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(e => e.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.HasIndex(e => new { e.ProjectId, e.ReportType, e.PeriodStart });
        builder.HasIndex(e => new { e.ProjectId, e.Status });

        builder.HasMany(e => e.Sections)
               .WithOne(e => e.ReportInstance)
               .HasForeignKey(e => e.ReportInstanceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Exports)
               .WithOne(e => e.ReportInstance)
               .HasForeignKey(e => e.ReportInstanceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
