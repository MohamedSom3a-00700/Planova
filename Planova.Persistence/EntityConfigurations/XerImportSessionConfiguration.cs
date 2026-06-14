using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using XerImportSession = Planova.Primavera.Domain.Entities.XerImportSession;

public class XerImportSessionConfiguration : IEntityTypeConfiguration<XerImportSession>
{
    public void Configure(EntityTypeBuilder<XerImportSession> builder)
    {
        builder.ToTable("XerImportSessions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.SourceFileName).HasMaxLength(500).IsRequired();
        builder.Property(e => e.SourceFileHash).HasMaxLength(64).IsRequired();
        builder.Property(e => e.ImportedBy).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.ProjectCode).HasMaxLength(200);
        builder.Property(e => e.ProjectName).HasMaxLength(500);
        builder.Property(e => e.TableNames);
        builder.HasIndex(e => new { e.SourceFileHash, e.ImportedAt });
    }
}
