using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using PrimaveraValidationRule = Planova.Primavera.Domain.Entities.PrimaveraValidationRule;

public class PrimaveraValidationRuleConfiguration : IEntityTypeConfiguration<PrimaveraValidationRule>
{
    public void Configure(EntityTypeBuilder<PrimaveraValidationRule> builder)
    {
        builder.ToTable("PrimaveraValidationRules");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Category).HasMaxLength(100).IsRequired();
    }
}
