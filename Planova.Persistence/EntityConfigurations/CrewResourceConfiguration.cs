using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Resource.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class CrewResourceConfiguration : IEntityTypeConfiguration<CrewResource>
{
    public void Configure(EntityTypeBuilder<CrewResource> builder)
    {
        builder.ToTable("CrewResources");
        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(cr => cr.IsLead).IsRequired();
        builder.Property(cr => cr.SortOrder).IsRequired();

        builder.HasIndex(cr => new { cr.CrewId, cr.ResourceId }).IsUnique();
    }
}
