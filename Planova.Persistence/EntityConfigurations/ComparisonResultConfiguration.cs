using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.ScheduleComparison.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ComparisonResultConfiguration : IEntityTypeConfiguration<ComparisonResult>
{
    public void Configure(EntityTypeBuilder<ComparisonResult> builder)
    {
        builder.ToTable("ComparisonResults");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.EntityType).HasMaxLength(30).IsRequired();
        builder.Property(e => e.MatchKey).HasMaxLength(200).IsRequired();
        builder.Property(e => e.FieldName).HasMaxLength(100);
        builder.Property(e => e.OldValue).HasMaxLength(2000);
        builder.Property(e => e.NewValue).HasMaxLength(2000);
        builder.Property(e => e.Severity).HasMaxLength(20).IsRequired();

        builder.Property(e => e.ChangeType)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(e => e.MatchConfidence)
               .HasConversion<string>()
               .HasMaxLength(10)
               .IsRequired();

        builder.HasIndex(e => new { e.SessionId, e.EntityType }).HasDatabaseName("IX_ComparisonResults_SessionId_EntityType");
    }
}
