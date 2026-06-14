using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

using PrimaveraCalendar = Planova.Primavera.Domain.Entities.PrimaveraCalendar;

public class PrimaveraCalendarConfiguration : IEntityTypeConfiguration<PrimaveraCalendar>
{
    public void Configure(EntityTypeBuilder<PrimaveraCalendar> builder)
    {
        builder.ToTable("PrimaveraCalendars");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.CalendarId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(e => e.ProjectId);
    }
}
