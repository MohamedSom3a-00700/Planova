using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using ActivityBankItemEntity = Planova.Activity.Domain.Entities.ActivityBankItem;

public class ActivityBankItemConfiguration : IEntityTypeConfiguration<ActivityBankItemEntity>
{
    public void Configure(EntityTypeBuilder<ActivityBankItemEntity> builder)
    {
        builder.ToTable("ActivityBankItems");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Code).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.DefaultActivityType).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(e => e.BankId);
        builder.HasIndex(e => e.ParentId);

        builder.HasOne(e => e.Bank)
               .WithMany(e => e.Items)
               .HasForeignKey(e => e.BankId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Parent)
               .WithMany(e => e.Children)
               .HasForeignKey(e => e.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
