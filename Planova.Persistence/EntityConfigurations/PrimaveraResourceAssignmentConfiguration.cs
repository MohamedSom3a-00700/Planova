using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using PrimaveraResourceAssignment = Planova.Primavera.Domain.Entities.PrimaveraResourceAssignment;

public class PrimaveraResourceAssignmentConfiguration : IEntityTypeConfiguration<PrimaveraResourceAssignment>
{
    public void Configure(EntityTypeBuilder<PrimaveraResourceAssignment> builder)
    {
        builder.ToTable("PrimaveraResourceAssignments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.TaskId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.ResourceId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.CostPerUnit).HasColumnType("decimal(18,4)");
        builder.HasIndex(e => e.ProjectId);
    }
}
