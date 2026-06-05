using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Resource.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ResourceRateConfiguration : IEntityTypeConfiguration<ResourceRate>
{
    public void Configure(EntityTypeBuilder<ResourceRate> builder)
    {
        builder.ToTable("ResourceRates");
        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.EffectiveDate).IsRequired();
        builder.Property(rr => rr.Rate).HasPrecision(18, 4).IsRequired();
        builder.Property(rr => rr.Currency).HasMaxLength(3).IsRequired();
        builder.Property(rr => rr.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(rr => rr.IsDefault).IsRequired();
        builder.Property(rr => rr.Notes).HasMaxLength(500);

        builder.HasIndex(rr => new { rr.ResourceId, rr.EffectiveDate }).IsUnique();
        builder.HasIndex(rr => new { rr.ResourceId, rr.EffectiveDate }).IsDescending(false, true);
    }
}
