using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Enums;
using Res = Planova.Resource.Domain.Entities.Resource;

namespace Planova.Resource.Application.Mappings;

public static class ResourceMappingProfile
{
    public static ResourceDto ToDto(this Res entity, decimal? effectiveRate = null)
    {
        return new ResourceDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            ResourceType = entity.ResourceType,
            Scope = entity.Scope,
            ProjectId = entity.ProjectId,
            Status = entity.Status,
            DefaultRate = entity.DefaultRate,
            UnitOfMeasure = entity.UnitOfMeasure,
            MaxQuantity = entity.MaxQuantity,
            Currency = entity.Currency,
            Description = entity.Description,
            Trade = entity.Trade,
            SkillLevel = entity.SkillLevel,
            EquipmentType = entity.EquipmentType,
            Capacity = entity.Capacity,
            OperatingCost = entity.OperatingCost,
            UnitPrice = entity.UnitPrice,
            WastagePercent = entity.WastagePercent,
            Company = entity.Company,
            ContractValue = entity.ContractValue,
            ContactName = entity.ContactName,
            ContactPhone = entity.ContactPhone,
            EffectiveRate = effectiveRate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static ResourceRateDto ToDto(this ResourceRate entity)
    {
        return new ResourceRateDto
        {
            Id = entity.Id,
            ResourceId = entity.ResourceId,
            EffectiveDate = entity.EffectiveDate,
            Rate = entity.Rate,
            Currency = entity.Currency,
            UnitOfMeasure = entity.UnitOfMeasure,
            IsDefault = entity.IsDefault,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt
        };
    }

    public static CrewDto ToDto(this Crew entity, decimal blendedRate = 0)
    {
        return new CrewDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ProjectId = entity.ProjectId,
            Status = entity.Status,
            Category = entity.Category,
            BlendedRate = blendedRate,
            ResourceCount = entity.Resources?.Count ?? 0,
            Resources = entity.Resources?.Select(r => r.ToDto()).ToList() ?? [],
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static CrewResourceDto ToDto(this CrewResource entity, decimal effectiveRate = 0)
    {
        return new CrewResourceDto
        {
            Id = entity.Id,
            CrewId = entity.CrewId,
            ResourceId = entity.ResourceId,
            ResourceCode = entity.Resource?.Code ?? string.Empty,
            ResourceName = entity.Resource?.Name ?? string.Empty,
            ResourceType = entity.Resource?.ResourceType ?? default,
            Quantity = entity.Quantity,
            IsLead = entity.IsLead,
            SortOrder = entity.SortOrder,
            EffectiveRate = effectiveRate,
            LineTotal = entity.Quantity * effectiveRate
        };
    }

    public static ResourceAssignmentDto ToDto(this ResourceAssignment entity)
    {
        return new ResourceAssignmentDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            ActivityId = entity.ActivityId,
            ResourceId = entity.ResourceId,
            ResourceCode = entity.Resource?.Code ?? string.Empty,
            ResourceName = entity.Resource?.Name ?? string.Empty,
            ResourceType = entity.Resource?.ResourceType ?? default,
            CrewId = entity.CrewId,
            Quantity = entity.Quantity,
            Rate = entity.Rate,
            Currency = entity.Currency,
            UnitOfMeasure = entity.UnitOfMeasure,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            TotalCost = entity.TotalCost,
            DurationDays = entity.DurationDays,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static Res ToEntity(this CreateResourceRequest request)
    {
        return new Res
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ResourceType = request.ResourceType,
            Scope = request.Scope,
            ProjectId = request.ProjectId,
            DefaultRate = request.DefaultRate,
            UnitOfMeasure = request.UnitOfMeasure,
            MaxQuantity = request.MaxQuantity,
            Currency = request.Currency,
            Description = request.Description,
            Trade = request.Trade,
            SkillLevel = request.SkillLevel,
            EquipmentType = request.EquipmentType,
            Capacity = request.Capacity,
            OperatingCost = request.OperatingCost,
            UnitPrice = request.UnitPrice,
            WastagePercent = request.WastagePercent,
            Company = request.Company,
            ContractValue = request.ContractValue,
            ContactName = request.ContactName,
            ContactPhone = request.ContactPhone
        };
    }
}
