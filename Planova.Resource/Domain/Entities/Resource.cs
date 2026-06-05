using Planova.Resource.Domain.Enums;

namespace Planova.Resource.Domain.Entities;

public class Resource
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public ResourceScope Scope { get; set; }
    public int? ProjectId { get; set; }
    public ResourceStatus Status { get; set; } = ResourceStatus.Active;
    public decimal DefaultRate { get; set; }
    public string UnitOfMeasure { get; set; } = "hr";
    public decimal? MaxQuantity { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }

    public bool IsGlobal => Scope == ResourceScope.Global;

    public string? Trade { get; set; }
    public string? SkillLevel { get; set; }
    public string? EquipmentType { get; set; }
    public string? Capacity { get; set; }
    public decimal? OperatingCost { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? WastagePercent { get; set; }
    public string? Company { get; set; }
    public decimal? ContractValue { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }

    public ICollection<ResourceRate> Rates { get; set; } = new List<ResourceRate>();
    public ICollection<CrewResource> CrewMemberships { get; set; } = new List<CrewResource>();
    public ICollection<ResourceAssignment> Assignments { get; set; } = new List<ResourceAssignment>();
    public ICollection<ResourceUsage> UsageRecords { get; set; } = new List<ResourceUsage>();
}
