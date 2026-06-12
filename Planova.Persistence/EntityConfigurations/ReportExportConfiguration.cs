using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Reporting.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ReportExportConfiguration : IEntityTypeConfiguration<ReportExport>
{
    public void Configure(EntityTypeBuilder<ReportExport> builder)
    {
        builder.ToTable("ReportExports");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Format)
               .HasConversion<string>()
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(e => e.FilePath).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.ExportedBy).HasMaxLength(100);
    }
}
