using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ProjectPartyLinkConfiguration : IEntityTypeConfiguration<ProjectPartyLink>
{
    public void Configure(EntityTypeBuilder<ProjectPartyLink> builder)
    {
        builder.ToTable("ProjectPartyLinks");

        builder.HasKey(e => new { e.ProjectId, e.PartyId });

        builder.Property(e => e.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Party)
            .WithMany()
            .HasForeignKey(e => e.PartyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ProjectId, e.PartyId })
            .IsUnique()
            .HasDatabaseName("IX_ProjectPartyLinks_ProjectId_PartyId");
    }
}
