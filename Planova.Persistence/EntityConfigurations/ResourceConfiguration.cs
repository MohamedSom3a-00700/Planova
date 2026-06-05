using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Resource.Domain.Enums;
using Res = Planova.Resource.Domain.Entities.Resource;

namespace Planova.Persistence.EntityConfigurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Res>
{
    public void Configure(EntityTypeBuilder<Res> builder)
    {
        builder.ToTable("Resources");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Code).HasMaxLength(20).IsRequired();
        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();
        builder.Property(r => r.ResourceType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Scope).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(r => r.DefaultRate).HasPrecision(18, 4).IsRequired();
        builder.Property(r => r.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(r => r.MaxQuantity).HasPrecision(18, 4);
        builder.Property(r => r.Currency).HasMaxLength(3).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.ProjectId);
        builder.Property(r => r.Trade).HasMaxLength(100);
        builder.Property(r => r.SkillLevel).HasMaxLength(50);
        builder.Property(r => r.EquipmentType).HasMaxLength(100);
        builder.Property(r => r.Capacity).HasMaxLength(100);
        builder.Property(r => r.OperatingCost).HasPrecision(18, 4);
        builder.Property(r => r.UnitPrice).HasPrecision(18, 4);
        builder.Property(r => r.WastagePercent).HasPrecision(5, 2);
        builder.Property(r => r.Company).HasMaxLength(200);
        builder.Property(r => r.ContractValue).HasPrecision(18, 2);
        builder.Property(r => r.ContactName).HasMaxLength(100);
        builder.Property(r => r.ContactPhone).HasMaxLength(50);
        builder.Ignore(r => r.IsGlobal);

        builder.HasIndex(r => new { r.Code, r.Scope, r.ProjectId }).IsUnique();
        builder.HasIndex(r => new { r.Scope, r.ProjectId });
        builder.HasIndex(r => new { r.ResourceType, r.Scope });
        builder.HasIndex(r => r.Name);

        builder.HasDiscriminator(r => r.ResourceType)
            .HasValue<Res>(ResourceType.Labour)
            .HasValue<Res>(ResourceType.Equipment)
            .HasValue<Res>(ResourceType.Material)
            .HasValue<Res>(ResourceType.Subcontractor);

        builder.HasMany(r => r.Rates).WithOne(rr => rr.Resource).HasForeignKey(rr => rr.ResourceId);
        builder.HasMany(r => r.CrewMemberships).WithOne(cr => cr.Resource).HasForeignKey(cr => cr.ResourceId);
        builder.HasMany(r => r.Assignments).WithOne(ra => ra.Resource).HasForeignKey(ra => ra.ResourceId);
        builder.HasMany(r => r.UsageRecords).WithOne(ru => ru.Resource).HasForeignKey(ru => ru.ResourceId);
    }
}
