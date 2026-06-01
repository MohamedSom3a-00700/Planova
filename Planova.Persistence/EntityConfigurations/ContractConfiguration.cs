using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Number)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Value)
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .HasMaxLength(10);

        builder.Property(e => e.Status)
            .HasMaxLength(30);

        builder.Property(e => e.Notes)
            .HasMaxLength(4000);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasIndex(e => e.Number)
            .IsUnique()
            .HasDatabaseName("IX_Contracts_Number");

        builder.HasIndex(e => e.ProjectId)
            .HasDatabaseName("IX_Contracts_ProjectId");

        builder.HasIndex(e => e.ClientId)
            .HasDatabaseName("IX_Contracts_ClientId");

        builder.HasIndex(e => e.UpdatedAt)
            .HasDatabaseName("IX_Contracts_UpdatedAt");

        builder.HasOne(e => e.Project)
            .WithMany(p => p.Contracts)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Client)
            .WithMany(c => c.Contracts)
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
