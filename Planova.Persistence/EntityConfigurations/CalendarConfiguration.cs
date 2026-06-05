using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using CalendarEntity = Planova.Activity.Domain.Entities.Calendar;

public class CalendarConfiguration : IEntityTypeConfiguration<CalendarEntity>
{
    public void Configure(EntityTypeBuilder<CalendarEntity> builder)
    {
        builder.ToTable("Calendars");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.HoursPerDay).HasColumnType("decimal(4,1)");
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasIndex(e => e.ProjectId);
    }
}
