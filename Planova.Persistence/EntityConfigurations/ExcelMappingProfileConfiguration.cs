using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ExcelMappingProfileConfiguration : IEntityTypeConfiguration<ExcelMappingProfile>
{
    public void Configure(EntityTypeBuilder<ExcelMappingProfile> builder)
    {
        builder.ToTable("ExcelMappingProfiles");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ColumnMappingsJson)
            .IsRequired()
            .HasColumnName("ColumnMappings");

        builder.Property(e => e.ValidationRulesJson)
            .IsRequired()
            .HasColumnName("ValidationRules");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(e => e.ModifiedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IX_ExcelMappingProfiles_Name");

        builder.HasIndex(e => e.EntityType)
            .HasDatabaseName("IX_ExcelMappingProfiles_EntityType");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
