using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using PrimaveraRelationship = Planova.Primavera.Domain.Entities.PrimaveraRelationship;

public class PrimaveraRelationshipConfiguration : IEntityTypeConfiguration<PrimaveraRelationship>
{
    public void Configure(EntityTypeBuilder<PrimaveraRelationship> builder)
    {
        builder.ToTable("PrimaveraRelationships");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.PredTaskId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.SuccTaskId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Type).HasMaxLength(10).IsRequired();
        builder.HasIndex(e => e.ProjectId);
    }
}
