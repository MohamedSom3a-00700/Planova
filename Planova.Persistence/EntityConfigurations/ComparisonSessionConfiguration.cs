using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.ScheduleComparison.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ComparisonSessionConfiguration : IEntityTypeConfiguration<ComparisonSession>
{
    public void Configure(EntityTypeBuilder<ComparisonSession> builder)
    {
        builder.ToTable("ComparisonSessions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.SourceKind).HasMaxLength(20).IsRequired();
        builder.Property(e => e.SourceLabel).HasMaxLength(200);
        builder.Property(e => e.TargetKind).HasMaxLength(20).IsRequired();
        builder.Property(e => e.TargetLabel).HasMaxLength(200);
        builder.Property(e => e.IncludedScopes).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Error).HasMaxLength(2000);

        builder.Property(e => e.Mode)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(e => e.State)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.HasMany(e => e.Results)
               .WithOne(e => e.Session)
               .HasForeignKey(e => e.SessionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ProjectId).HasDatabaseName("IX_ComparisonSessions_ProjectId");
    }
}
