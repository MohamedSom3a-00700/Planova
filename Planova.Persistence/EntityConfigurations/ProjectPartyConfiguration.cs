using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Reporting.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ProjectPartyConfiguration : IEntityTypeConfiguration<ProjectParty>
{
    public void Configure(EntityTypeBuilder<ProjectParty> builder)
    {
        builder.ToTable("ProjectParties");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.LogoPath).HasMaxLength(1000);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.ContactPerson).HasMaxLength(100);
        builder.Property(e => e.ContactEmail).HasMaxLength(100);
        builder.Property(e => e.ContactPhone).HasMaxLength(50);

        builder.Property(e => e.Role)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.HasIndex(e => new { e.ProjectId, e.Role })
               .IsUnique()
               .HasFilter("[Role] IN ('Client', 'MainContractor')");
    }
}
