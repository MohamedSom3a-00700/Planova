using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Boq.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class BoqImportSessionConfiguration : IEntityTypeConfiguration<BoqImportSession>
{
    public void Configure(EntityTypeBuilder<BoqImportSession> builder)
    {
        builder.ToTable("BoqImportSessions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.FilePath)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.ImportMode)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.ImportedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);

        builder.HasMany(e => e.WorksheetMappings)
            .WithOne(e => e.ImportSession)
            .HasForeignKey(e => e.ImportSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
