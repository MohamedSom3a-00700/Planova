using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using PrimaveraCode = Planova.Primavera.Domain.Entities.PrimaveraCode;

public class PrimaveraCodeConfiguration : IEntityTypeConfiguration<PrimaveraCode>
{
    public void Configure(EntityTypeBuilder<PrimaveraCode> builder)
    {
        builder.ToTable("PrimaveraCodes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.CodeType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.CodeTypeId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.CodeValue).HasMaxLength(200).IsRequired();
        builder.Property(e => e.CodeName).HasMaxLength(500).IsRequired();
        builder.HasIndex(e => e.ProjectId);
    }
}
