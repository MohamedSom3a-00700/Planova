using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class SubcontractorConfiguration : IEntityTypeConfiguration<Subcontractor>
{
    public void Configure(EntityTypeBuilder<Subcontractor> builder)
    {
        builder.ToTable("Subcontractors");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ContactEmail)
            .HasMaxLength(200);

        builder.Property(e => e.ContactPhone)
            .HasMaxLength(50);

        builder.Property(e => e.OrganizationDetails)
            .HasMaxLength(2000);

        builder.Property(e => e.Trade)
            .HasMaxLength(100);

        builder.Property(e => e.LicenseNumber)
            .HasMaxLength(100);

        builder.Property(e => e.Logo)
            .HasMaxLength(500);

        builder.Property(e => e.Notes)
            .HasMaxLength(4000);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("IX_Subcontractors_Code");

        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("IX_Subcontractors_Name");

        builder.HasIndex(e => e.UpdatedAt)
            .HasDatabaseName("IX_Subcontractors_UpdatedAt");

        builder.HasMany(e => e.Projects)
            .WithOne(p => p.Subcontractor)
            .HasForeignKey(p => p.SubcontractorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}