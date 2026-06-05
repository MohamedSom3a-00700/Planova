using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Resource.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ResourceAssignmentConfiguration : IEntityTypeConfiguration<ResourceAssignment>
{
    public void Configure(EntityTypeBuilder<ResourceAssignment> builder)
    {
        builder.ToTable("ResourceAssignments");
        builder.HasKey(ra => ra.Id);

        builder.Property(ra => ra.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(ra => ra.Rate).HasPrecision(18, 4).IsRequired();
        builder.Property(ra => ra.Currency).HasMaxLength(3).IsRequired();
        builder.Property(ra => ra.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(ra => ra.TotalCost).HasPrecision(18, 2).IsRequired();
        builder.Property(ra => ra.DurationDays).HasPrecision(18, 2);
        builder.Property(ra => ra.Notes).HasMaxLength(500);

        builder.HasIndex(ra => ra.ActivityId);
        builder.HasIndex(ra => new { ra.ProjectId, ra.ResourceId });
    }
}
