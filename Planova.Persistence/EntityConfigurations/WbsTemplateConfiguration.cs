using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using WbsTemplateEntity = Planova.Wbs.Domain.Entities.WbsTemplate;

public class WbsTemplateConfiguration : IEntityTypeConfiguration<WbsTemplateEntity>
{
    public void Configure(EntityTypeBuilder<WbsTemplateEntity> builder)
    {
        builder.ToTable("WbsTemplates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Industry)
            .HasMaxLength(100);

        builder.Property(e => e.Tags)
            .HasMaxLength(2000);

        builder.Property(e => e.Version)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasMany(e => e.Items)
            .WithOne(e => e.Template)
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
