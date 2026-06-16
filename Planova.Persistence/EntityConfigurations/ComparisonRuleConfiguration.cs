using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.ScheduleComparison.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ComparisonRuleConfiguration : IEntityTypeConfiguration<ComparisonRule>
{
    public void Configure(EntityTypeBuilder<ComparisonRule> builder)
    {
        builder.ToTable("ComparisonRules");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.MatchingStrategyPreference).HasMaxLength(500);

        builder.HasIndex(e => e.ProjectId).HasDatabaseName("IX_ComparisonRules_ProjectId");
    }
}
