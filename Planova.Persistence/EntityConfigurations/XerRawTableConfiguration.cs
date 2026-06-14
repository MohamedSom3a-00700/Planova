using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using XerRawTable = Planova.Primavera.Domain.Entities.XerRawTable;

public class XerRawTableConfiguration : IEntityTypeConfiguration<XerRawTable>
{
    public void Configure(EntityTypeBuilder<XerRawTable> builder)
    {
        builder.ToTable("XerRawTables");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.TableName).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.ImportSessionId);
    }
}
