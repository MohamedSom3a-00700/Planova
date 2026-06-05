using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using ActivityEntity = Planova.Activity.Domain.Entities.Activity;

public class ActivityConfiguration : IEntityTypeConfiguration<ActivityEntity>
{
    public void Configure(EntityTypeBuilder<ActivityEntity> builder)
    {
        builder.ToTable("Activities");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Code).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Notes).HasMaxLength(4000);
        builder.Property(e => e.Weight).HasColumnType("decimal(5,2)");
        builder.Property(e => e.PercentComplete).HasColumnType("decimal(5,2)");
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ActivityType).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.WbsItemId);
        builder.HasIndex(e => e.CalendarId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ParentActivityId);
        builder.HasIndex(e => new { e.ProjectId, e.Code }).IsUnique();

        builder.HasOne(e => e.ParentActivity)
               .WithMany(e => e.Children)
               .HasForeignKey(e => e.ParentActivityId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
