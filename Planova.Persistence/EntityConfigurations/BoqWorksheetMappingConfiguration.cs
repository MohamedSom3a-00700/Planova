using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Boq.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class BoqWorksheetMappingConfiguration : IEntityTypeConfiguration<BoqWorksheetMapping>
{
    public void Configure(EntityTypeBuilder<BoqWorksheetMapping> builder)
    {
        builder.ToTable("BoqWorksheetMappings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.WorksheetName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ColumnMappings)
            .IsRequired();

        builder.Property(e => e.MatchConfidence)
            .HasPrecision(5, 4);

        builder.Property(e => e.SectionAmount)
            .HasPrecision(18, 2);
    }
}
