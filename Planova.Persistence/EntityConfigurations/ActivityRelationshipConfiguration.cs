using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using ActivityRelationshipEntity = Planova.Activity.Domain.Entities.ActivityRelationship;

public class ActivityRelationshipConfiguration : IEntityTypeConfiguration<ActivityRelationshipEntity>
{
    public void Configure(EntityTypeBuilder<ActivityRelationshipEntity> builder)
    {
        builder.ToTable("ActivityRelationships");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(10);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.PredecessorId);
        builder.HasIndex(e => e.SuccessorId);

        builder.HasOne(e => e.Predecessor)
               .WithMany()
               .HasForeignKey(e => e.PredecessorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Successor)
               .WithMany()
               .HasForeignKey(e => e.SuccessorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
