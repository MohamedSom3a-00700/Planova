using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Reporting.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ReportSettingsConfiguration : IEntityTypeConfiguration<ReportSettings>
{
    public void Configure(EntityTypeBuilder<ReportSettings> builder)
    {
        builder.ToTable("ReportSettings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.EnabledSectionsJson).IsRequired();

        builder.Property(e => e.ReportType)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.HasIndex(e => new { e.ProjectId, e.ReportType }).IsUnique();
    }
}
