using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Reporting.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ReportSectionConfiguration : IEntityTypeConfiguration<ReportSection>
{
    public void Configure(EntityTypeBuilder<ReportSection> builder)
    {
        builder.ToTable("ReportSections");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ContentJson).IsRequired();

        builder.Property(e => e.SectionType)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();
    }
}
