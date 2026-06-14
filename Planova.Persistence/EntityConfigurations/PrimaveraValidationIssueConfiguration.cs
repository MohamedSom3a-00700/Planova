using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using PrimaveraValidationIssue = Planova.Primavera.Domain.Entities.PrimaveraValidationIssue;

public class PrimaveraValidationIssueConfiguration : IEntityTypeConfiguration<PrimaveraValidationIssue>
{
    public void Configure(EntityTypeBuilder<PrimaveraValidationIssue> builder)
    {
        builder.ToTable("PrimaveraValidationIssues");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.EntityType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.RuleId);
    }
}
