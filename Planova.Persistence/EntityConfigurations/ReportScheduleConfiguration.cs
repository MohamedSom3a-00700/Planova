using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Reporting.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ReportScheduleConfiguration : IEntityTypeConfiguration<ReportSchedule>
{
    public void Configure(EntityTypeBuilder<ReportSchedule> builder)
    {
        builder.ToTable("ReportSchedules");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.ReportType)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(e => e.Frequency)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(e => e.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.ExportFormats).IsRequired();
        builder.Property(e => e.LastStatus).HasMaxLength(20);
        builder.Property(e => e.LastErrorMessage).HasMaxLength(2000);

        builder.HasIndex(e => new { e.ProjectId, e.ReportType }).IsUnique();
    }
}
