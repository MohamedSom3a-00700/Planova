using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Planova.Persistence.EntityConfigurations;

public class BoqClassificationConfiguration : IEntityTypeConfiguration<Planova.Boq.Domain.Entities.BoqClassification>
{
    public void Configure(EntityTypeBuilder<Planova.Boq.Domain.Entities.BoqClassification> builder)
    {
        builder.ToTable("BoqClassifications");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Scope)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(e => new { e.Code, e.Scope })
            .IsUnique()
            .HasDatabaseName("IX_BoqClassifications_Code_Scope");

        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
