using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ThemePreference).HasDefaultValue("Dark");
        builder.Property(e => e.LanguagePreference).HasDefaultValue("en");
        builder.Property(e => e.WindowX).IsRequired(false);
        builder.Property(e => e.WindowY).IsRequired(false);
        builder.Property(e => e.WindowWidth).IsRequired(false);
        builder.Property(e => e.WindowHeight).IsRequired(false);
        builder.Property(e => e.WindowMaximized).HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");
    }
}
