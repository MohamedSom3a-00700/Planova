using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using WbsTemplateItemEntity = Planova.Wbs.Domain.Entities.WbsTemplateItem;

public class WbsTemplateItemConfiguration : IEntityTypeConfiguration<WbsTemplateItemEntity>
{
    public void Configure(EntityTypeBuilder<WbsTemplateItemEntity> builder)
    {
        builder.ToTable("WbsTemplateItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ShortCode)
            .HasMaxLength(10);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.WbsLevel)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.DefaultDurationDays)
            .HasDefaultValue(null);

        builder.Property(e => e.TypicalWeight)
            .HasPrecision(5, 2);

        builder.HasOne(e => e.Template)
            .WithMany(e => e.Items)
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
