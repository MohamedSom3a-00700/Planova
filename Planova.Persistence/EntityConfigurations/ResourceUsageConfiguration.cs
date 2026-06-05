using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Resource.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ResourceUsageConfiguration : IEntityTypeConfiguration<ResourceUsage>
{
    public void Configure(EntityTypeBuilder<ResourceUsage> builder)
    {
        builder.ToTable("ResourceUsages");
        builder.HasKey(ru => ru.Id);

        builder.Property(ru => ru.Date).IsRequired();
        builder.Property(ru => ru.PlannedQuantity).HasPrecision(18, 4).IsRequired();
        builder.Property(ru => ru.ActualQuantity).HasPrecision(18, 4);

        builder.HasIndex(ru => new { ru.ResourceId, ru.Date });
        builder.HasIndex(ru => new { ru.Date, ru.ResourceId });
    }
}
