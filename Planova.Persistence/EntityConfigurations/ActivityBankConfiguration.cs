using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using ActivityBankEntity = Planova.Activity.Domain.Entities.ActivityBank;

public class ActivityBankConfiguration : IEntityTypeConfiguration<ActivityBankEntity>
{
    public void Configure(EntityTypeBuilder<ActivityBankEntity> builder)
    {
        builder.ToTable("ActivityBanks");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Category).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Subcategory).HasMaxLength(100);
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Tags).HasMaxLength(2000);

        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => new { e.Category, e.Code }).IsUnique();
    }
}
