using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using CalendarDayEntity = Planova.Activity.Domain.Entities.CalendarDay;

public class CalendarDayConfiguration : IEntityTypeConfiguration<CalendarDayEntity>
{
    public void Configure(EntityTypeBuilder<CalendarDayEntity> builder)
    {
        builder.ToTable("CalendarDays");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Label).HasMaxLength(200);

        builder.HasIndex(e => new { e.CalendarId, e.Date }).IsUnique();

        builder.HasOne(e => e.Calendar)
               .WithMany(e => e.Days)
               .HasForeignKey(e => e.CalendarId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
