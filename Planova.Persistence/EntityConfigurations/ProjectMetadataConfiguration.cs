using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ProjectMetadataConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Property(e => e.CoverImagePath)
            .HasMaxLength(1000);

        builder.Property(e => e.ProjectFolderPath)
            .HasMaxLength(1000);

        builder.Property(e => e.ConnectedXerPath)
            .HasMaxLength(1000);

        builder.Property(e => e.ConnectedDatabase)
            .HasMaxLength(200);

        builder.Property(e => e.GoogleMapsLink)
            .HasMaxLength(2000);

        builder.Property(e => e.ProgressPercentage)
            .HasPrecision(5, 2);

        builder.Property(e => e.HealthIndicator)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("Green");

        builder.Property(e => e.RiskLevel)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("Low");

        builder.Property(e => e.Budget)
            .HasPrecision(18, 2);

        builder.Property(e => e.OriginalBudget)
            .HasPrecision(18, 2);

        builder.Property(e => e.CurrentBudget)
            .HasPrecision(18, 2);

        builder.Property(e => e.ActualCost)
            .HasPrecision(18, 2);

        builder.Property(e => e.EarnedValue)
            .HasPrecision(18, 2);

        builder.Property(e => e.Cpi)
            .HasPrecision(10, 4);

        builder.Property(e => e.Spi)
            .HasPrecision(10, 4);

        builder.Property(e => e.ScheduleHealthPct)
            .HasPrecision(5, 2);

        builder.Property(e => e.CostHealthPct)
            .HasPrecision(5, 2);
    }
}
