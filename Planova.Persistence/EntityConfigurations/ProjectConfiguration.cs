using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(e => e.Currency)
            .HasMaxLength(10);

        builder.Property(e => e.Location)
            .HasMaxLength(200);

        builder.Property(e => e.Notes)
            .HasMaxLength(4000);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("IX_Projects_Code");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Projects_Status");

        builder.HasIndex(e => e.ClientId)
            .HasDatabaseName("IX_Projects_ClientId");

        builder.HasIndex(e => e.UpdatedAt)
            .HasDatabaseName("IX_Projects_UpdatedAt");

        builder.HasOne(e => e.Client)
            .WithMany(c => c.Projects)
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Contracts)
            .WithOne(c => c.Project)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
