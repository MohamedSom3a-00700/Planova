using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Resource.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class CrewConfiguration : IEntityTypeConfiguration<Crew>
{
    public void Configure(EntityTypeBuilder<Crew> builder)
    {
        builder.ToTable("Crews");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(c => c.Category).HasMaxLength(100);

        builder.HasMany(c => c.Resources).WithOne(cr => cr.Crew).HasForeignKey(cr => cr.CrewId);
    }
}
