using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

public class BoqLibraryConfiguration : IEntityTypeConfiguration<Planova.Boq.Domain.Entities.BoqLibrary>
{
    public void Configure(EntityTypeBuilder<Planova.Boq.Domain.Entities.BoqLibrary> builder)
    {
        builder.ToTable("BoqLibraries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.LibraryType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(e => e.ModifiedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasMany(e => e.Items)
            .WithOne(e => e.Library)
            .HasForeignKey(e => e.LibraryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BoqLibraryItemConfiguration : IEntityTypeConfiguration<Planova.Boq.Domain.Entities.BoqLibraryItem>
{
    public void Configure(EntityTypeBuilder<Planova.Boq.Domain.Entities.BoqLibraryItem> builder)
    {
        builder.ToTable("BoqLibraryItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.DefaultRate)
            .HasPrecision(18, 4);

        builder.Property(e => e.Category)
            .HasMaxLength(100);

        builder.Property(e => e.Tags)
            .HasMaxLength(500);

        builder.HasIndex(e => e.LibraryId)
            .HasDatabaseName("IX_BoqLibraryItems_LibraryId");
    }
}
