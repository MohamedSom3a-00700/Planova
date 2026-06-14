using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using PrimaveraUdf = Planova.Primavera.Domain.Entities.PrimaveraUdf;

public class PrimaveraUdfConfiguration : IEntityTypeConfiguration<PrimaveraUdf>
{
    public void Configure(EntityTypeBuilder<PrimaveraUdf> builder)
    {
        builder.ToTable("PrimaveraUdfs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.UdfTypeId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.TableName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.FieldName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.FieldType).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.ProjectId);
    }
}
