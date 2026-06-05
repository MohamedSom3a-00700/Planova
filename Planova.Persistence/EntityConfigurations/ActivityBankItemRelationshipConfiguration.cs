using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using ActivityBankItemRelationshipEntity = Planova.Activity.Domain.Entities.ActivityBankItemRelationship;

public class ActivityBankItemRelationshipConfiguration : IEntityTypeConfiguration<ActivityBankItemRelationshipEntity>
{
    public void Configure(EntityTypeBuilder<ActivityBankItemRelationshipEntity> builder)
    {
        builder.ToTable("ActivityBankItemRelationships");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(10);

        builder.HasIndex(e => e.BankId);

        builder.HasOne(e => e.Bank)
               .WithMany(e => e.Relationships)
               .HasForeignKey(e => e.BankId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PredecessorItem)
               .WithMany()
               .HasForeignKey(e => e.PredecessorItemId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SuccessorItem)
               .WithMany()
               .HasForeignKey(e => e.SuccessorItemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
