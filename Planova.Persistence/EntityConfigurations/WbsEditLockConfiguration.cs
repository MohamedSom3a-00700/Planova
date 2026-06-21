using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Wbs.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class WbsEditLockConfiguration : IEntityTypeConfiguration<WbsEditLock>
{
    public void Configure(EntityTypeBuilder<WbsEditLock> builder)
    {
        builder.ToTable("WbsEditLocks");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.LockedByUserId)
            .IsRequired();

        builder.Property(e => e.LockedAt)
            .IsRequired();

        builder.Property(e => e.LockExpiresAt)
            .IsRequired();

        builder.HasOne(e => e.Wbs)
            .WithOne()
            .HasForeignKey<WbsEditLock>(e => e.WbsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.WbsId)
            .IsUnique()
            .HasDatabaseName("IX_WbsEditLocks_WbsId");
    }
}
