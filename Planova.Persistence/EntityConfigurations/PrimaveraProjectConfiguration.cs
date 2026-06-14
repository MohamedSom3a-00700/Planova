using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using PrimaveraProject = Planova.Primavera.Domain.Entities.PrimaveraProject;

public class PrimaveraProjectConfiguration : IEntityTypeConfiguration<PrimaveraProject>
{
    public void Configure(EntityTypeBuilder<PrimaveraProject> builder)
    {
        builder.ToTable("PrimaveraProjects");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.ProjectId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(500).IsRequired();
        builder.Property(e => e.SourceFileName).HasMaxLength(500).IsRequired();
        builder.Property(e => e.ImportedAt).IsRequired();
        builder.HasIndex(e => e.ProjectId);
    }
}
