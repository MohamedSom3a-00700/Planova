using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ConsultantConfiguration : IEntityTypeConfiguration<Consultant>
{
    public void Configure(EntityTypeBuilder<Consultant> builder)
    {
        builder.ToTable("Consultants");

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
            .HasDatabaseName("IX_Consultants_Code");

        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("IX_Consultants_Name");

        builder.HasIndex(e => e.UpdatedAt)
            .HasDatabaseName("IX_Consultants_UpdatedAt");

        builder.HasMany(e => e.Projects)
            .WithOne(p => p.Consultant)
            .HasForeignKey(p => p.ConsultantId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
