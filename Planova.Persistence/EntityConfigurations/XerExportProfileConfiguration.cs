using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using XerExportProfile = Planova.Primavera.Domain.Entities.XerExportProfile;

public class XerExportProfileConfiguration : IEntityTypeConfiguration<XerExportProfile>
{
    public void Configure(EntityTypeBuilder<XerExportProfile> builder)
    {
        builder.ToTable("XerExportProfiles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
    }
}
