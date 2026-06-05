using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Persistence.Repositories;

using WbsLevelType = Planova.Wbs.Domain.Enums.WbsLevelType;

public class WbsTemplateRepository : IWbsTemplateRepository
{
    private readonly List<WbsTemplate> _store = new();
    private readonly object _lock = new();
    private bool _seeded;

    public WbsTemplateRepository()
    {
        SeedIfEmpty();
    }

    private void SeedIfEmpty()
    {
        if (_seeded) return;
        _seeded = true;

        var templates = new List<WbsTemplate>
        {
            CreateBuildingConstructionTemplate(),
            CreateResidentialBuildingTemplate(),
            CreateCommercialComplexTemplate(),
            CreateInfrastructureTemplate(),
            CreateRoadsAndBridgesTemplate(),
            CreateWaterTreatmentTemplate(),
            CreateIndustrialTemplate(),
            CreatePowerPlantTemplate(),
            CreateOilAndGasTemplate(),
            CreateTelecommunicationsTemplate()
        };

        foreach (var t in templates)
        {
            AddStandardMilestones(t);
            _store.Add(t);
        }
    }

    public Task<WbsTemplate> GetByIdAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            var entity = _store.FirstOrDefault(e => e.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"WbsTemplate {id} not found");
            return Task.FromResult(entity);
        }
    }

    public Task<IReadOnlyList<WbsTemplate>> GetAllAsync(CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<WbsTemplate>>(
                _store.OrderBy(e => e.Category).ThenBy(e => e.Name).ToList());
        }
    }

    public Task<IReadOnlyList<WbsTemplate>> GetByCategoryAsync(string category, CancellationToken ct)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<WbsTemplate>>(
                _store.Where(e => e.Category == category)
                    .OrderBy(e => e.Name)
                    .ToList());
        }
    }

    public Task<WbsTemplate> AddAsync(WbsTemplate template, CancellationToken ct)
    {
        lock (_lock)
        {
            template.Id = Guid.NewGuid();
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;
            _store.Add(template);
            return Task.FromResult(template);
        }
    }

    public Task<WbsTemplate> UpdateAsync(WbsTemplate template, CancellationToken ct)
    {
        lock (_lock)
        {
            var existing = _store.FirstOrDefault(e => e.Id == template.Id);
            if (existing != null)
            {
                _store.Remove(existing);
                template.UpdatedAt = DateTime.UtcNow;
                _store.Add(template);
            }
            return Task.FromResult(template);
        }
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        lock (_lock)
        {
            _store.RemoveAll(e => e.Id == id);
        }
        return Task.CompletedTask;
    }

    private static WbsTemplate CreateBuildingConstructionTemplate()
    {
        var template = new WbsTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Building Construction",
            Category = "Building Construction",
            Industry = "Construction",
            Description = "Standard WBS for building construction projects including substructure, superstructure, finishes, and services",
            IsStandard = true,
            Version = 1,
            Tags = "[\"building\", \"construction\", \"residential\", \"commercial\"]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var itemId = Guid.NewGuid;
        var i1 = itemId(); var i1a = itemId(); var i1b = itemId(); var i1c = itemId();
        var i2 = itemId(); var i2a = itemId(); var i2b = itemId(); var i2c = itemId();
        var i3 = itemId(); var i3a = itemId(); var i3b = itemId();
        var i4 = itemId(); var i4a = itemId(); var i4b = itemId();

        template.Items = new List<WbsTemplateItem>
        {
            new() { Id = i1, TemplateId = template.Id, ParentId = null, Name = "Substructure", Code = "1", ShortCode = "SUB", Level = 0, SortOrder = 1, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 30, TypicalWeight = 15 },
            new() { Id = i1a, TemplateId = template.Id, ParentId = i1, Name = "Foundation", Code = "1.1", ShortCode = "FND", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 8 },
            new() { Id = i1b, TemplateId = template.Id, ParentId = i1, Name = "Basement Construction", Code = "1.2", ShortCode = "BSM", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 7 },
            new() { Id = i1c, TemplateId = template.Id, ParentId = i1b, Name = "Waterproofing", Code = "1.2.1", ShortCode = "WPR", Level = 2, SortOrder = 1, WbsLevel = WbsLevelType.WorkPackage, DefaultDurationDays = 5, TypicalWeight = 3 },
            new() { Id = i2, TemplateId = template.Id, ParentId = null, Name = "Superstructure", Code = "2", ShortCode = "SUP", Level = 0, SortOrder = 2, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 60, TypicalWeight = 30 },
            new() { Id = i2a, TemplateId = template.Id, ParentId = i2, Name = "Structural Frame", Code = "2.1", ShortCode = "STR", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 15 },
            new() { Id = i2b, TemplateId = template.Id, ParentId = i2, Name = "Floor Slabs", Code = "2.2", ShortCode = "FLR", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 10 },
            new() { Id = i2c, TemplateId = template.Id, ParentId = i2, Name = "Roof Structure", Code = "2.3", ShortCode = "ROF", Level = 1, SortOrder = 3, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 10, TypicalWeight = 5 },
            new() { Id = i3, TemplateId = template.Id, ParentId = null, Name = "Finishes", Code = "3", ShortCode = "FIN", Level = 0, SortOrder = 3, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 45, TypicalWeight = 25 },
            new() { Id = i3a, TemplateId = template.Id, ParentId = i3, Name = "Interior Finishes", Code = "3.1", ShortCode = "INT", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 25, TypicalWeight = 15 },
            new() { Id = i3b, TemplateId = template.Id, ParentId = i3, Name = "Exterior Finishes", Code = "3.2", ShortCode = "EXT", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 10 },
            new() { Id = i4, TemplateId = template.Id, ParentId = null, Name = "Building Services", Code = "4", ShortCode = "SVC", Level = 0, SortOrder = 4, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 50, TypicalWeight = 30 },
            new() { Id = i4a, TemplateId = template.Id, ParentId = i4, Name = "Electrical", Code = "4.1", ShortCode = "ELE", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 12 },
            new() { Id = i4b, TemplateId = template.Id, ParentId = i4, Name = "Plumbing & HVAC", Code = "4.2", ShortCode = "PLB", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 18 }
        };

        return template;
    }

    private static WbsTemplate CreateInfrastructureTemplate()
    {
        var template = new WbsTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Infrastructure Project",
            Category = "Infrastructure",
            Industry = "Civil Engineering",
            Description = "WBS for infrastructure projects including roads, bridges, utilities, and earthworks",
            IsStandard = true,
            Version = 1,
            Tags = "[\"infrastructure\", \"civil\", \"road\", \"bridge\", \"utility\"]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var itemId = Guid.NewGuid;
        var i1 = itemId(); var i1a = itemId(); var i1b = itemId();
        var i2 = itemId(); var i2a = itemId(); var i2b = itemId();
        var i3 = itemId(); var i3a = itemId(); var i3b = itemId();

        template.Items = new List<WbsTemplateItem>
        {
            new() { Id = i1, TemplateId = template.Id, ParentId = null, Name = "Earthworks", Code = "1", ShortCode = "ERT", Level = 0, SortOrder = 1, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 40, TypicalWeight = 20 },
            new() { Id = i1a, TemplateId = template.Id, ParentId = i1, Name = "Site Clearance", Code = "1.1", ShortCode = "CLR", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 10, TypicalWeight = 5 },
            new() { Id = i1b, TemplateId = template.Id, ParentId = i1, Name = "Excavation & Fill", Code = "1.2", ShortCode = "EXC", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 15 },
            new() { Id = i2, TemplateId = template.Id, ParentId = null, Name = "Pavement & Surfacing", Code = "2", ShortCode = "PAV", Level = 0, SortOrder = 2, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 50, TypicalWeight = 35 },
            new() { Id = i2a, TemplateId = template.Id, ParentId = i2, Name = "Base Course", Code = "2.1", ShortCode = "BAS", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 15 },
            new() { Id = i2b, TemplateId = template.Id, ParentId = i2, Name = "Asphalt/Concrete", Code = "2.2", ShortCode = "ASP", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 20 },
            new() { Id = i3, TemplateId = template.Id, ParentId = null, Name = "Utilities & Drainage", Code = "3", ShortCode = "UTL", Level = 0, SortOrder = 3, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 35, TypicalWeight = 25 },
            new() { Id = i3a, TemplateId = template.Id, ParentId = i3, Name = "Stormwater Drainage", Code = "3.1", ShortCode = "DRN", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 15 },
            new() { Id = i3b, TemplateId = template.Id, ParentId = i3, Name = "Water & Sewer", Code = "3.2", ShortCode = "WTR", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 10 }
        };

        return template;
    }

    private static WbsTemplate CreateIndustrialTemplate()
    {
        var template = new WbsTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Industrial Facility",
            Category = "Industrial",
            Industry = "Industrial Engineering",
            Description = "WBS for industrial facility construction including process areas, utilities, and commissioning",
            IsStandard = true,
            Version = 1,
            Tags = "[\"industrial\", \"facility\", \"process\", \"manufacturing\"]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var itemId = Guid.NewGuid;
        var i1 = itemId(); var i1a = itemId(); var i1b = itemId();
        var i2 = itemId(); var i2a = itemId(); var i2b = itemId();
        var i3 = itemId(); var i3a = itemId(); var i3b = itemId();

        template.Items = new List<WbsTemplateItem>
        {
            new() { Id = i1, TemplateId = template.Id, ParentId = null, Name = "Process Areas", Code = "1", ShortCode = "PRO", Level = 0, SortOrder = 1, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 60, TypicalWeight = 35 },
            new() { Id = i1a, TemplateId = template.Id, ParentId = i1, Name = "Production Building", Code = "1.1", ShortCode = "PRD", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 35, TypicalWeight = 20 },
            new() { Id = i1b, TemplateId = template.Id, ParentId = i1, Name = "Warehouse & Storage", Code = "1.2", ShortCode = "WRH", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 25, TypicalWeight = 15 },
            new() { Id = i2, TemplateId = template.Id, ParentId = null, Name = "Utilities", Code = "2", ShortCode = "UTL", Level = 0, SortOrder = 2, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 40, TypicalWeight = 30 },
            new() { Id = i2a, TemplateId = template.Id, ParentId = i2, Name = "Electrical Supply", Code = "2.1", ShortCode = "ELC", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 15 },
            new() { Id = i2b, TemplateId = template.Id, ParentId = i2, Name = "HVAC & Compressed Air", Code = "2.2", ShortCode = "HVC", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 15 },
            new() { Id = i3, TemplateId = template.Id, ParentId = null, Name = "Commissioning", Code = "3", ShortCode = "COM", Level = 0, SortOrder = 3, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 30, TypicalWeight = 20 },
            new() { Id = i3a, TemplateId = template.Id, ParentId = i3, Name = "Equipment Installation", Code = "3.1", ShortCode = "EQP", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 12 },
            new() { Id = i3b, TemplateId = template.Id, ParentId = i3, Name = "Testing & Handover", Code = "3.2", ShortCode = "TST", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 10, TypicalWeight = 8 }
        };

        return template;
    }

    private static WbsTemplate CreateResidentialBuildingTemplate()
    {
        var template = new WbsTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Residential Building",
            Category = "Building Construction",
            Industry = "Construction",
            Description = "WBS for multi-story residential building projects including apartments, amenities, and landscaping",
            IsStandard = true,
            Version = 1,
            Tags = "[\"residential\", \"building\", \"apartment\", \"housing\"]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var itemId = Guid.NewGuid;
        var i1 = itemId(); var i1a = itemId(); var i1b = itemId();
        var i2 = itemId(); var i2a = itemId(); var i2b = itemId(); var i2c = itemId();
        var i3 = itemId(); var i3a = itemId(); var i3b = itemId();
        var i4 = itemId(); var i4a = itemId(); var i4b = itemId(); var i4c = itemId();

        template.Items = new List<WbsTemplateItem>
        {
            new() { Id = i1, TemplateId = template.Id, ParentId = null, Name = "Site Works", Code = "1", ShortCode = "SIT", Level = 0, SortOrder = 1, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 20, TypicalWeight = 10 },
            new() { Id = i1a, TemplateId = template.Id, ParentId = i1, Name = "Site Clearance & Earthworks", Code = "1.1", ShortCode = "CLR", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 10, TypicalWeight = 5 },
            new() { Id = i1b, TemplateId = template.Id, ParentId = i1, Name = "Foundations", Code = "1.2", ShortCode = "FND", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 10, TypicalWeight = 5 },
            new() { Id = i2, TemplateId = template.Id, ParentId = null, Name = "Building Structure", Code = "2", ShortCode = "STR", Level = 0, SortOrder = 2, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 90, TypicalWeight = 35 },
            new() { Id = i2a, TemplateId = template.Id, ParentId = i2, Name = "Basement & Parking", Code = "2.1", ShortCode = "BSM", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 25, TypicalWeight = 10 },
            new() { Id = i2b, TemplateId = template.Id, ParentId = i2, Name = "Structural Frame", Code = "2.2", ShortCode = "FRM", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 40, TypicalWeight = 15 },
            new() { Id = i2c, TemplateId = template.Id, ParentId = i2, Name = "Roof & Top Floor", Code = "2.3", ShortCode = "ROF", Level = 1, SortOrder = 3, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 25, TypicalWeight = 10 },
            new() { Id = i3, TemplateId = template.Id, ParentId = null, Name = "Interior Finishes", Code = "3", ShortCode = "INT", Level = 0, SortOrder = 3, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 60, TypicalWeight = 30 },
            new() { Id = i3a, TemplateId = template.Id, ParentId = i3, Name = "Wall & Floor Finishes", Code = "3.1", ShortCode = "WFL", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 15 },
            new() { Id = i3b, TemplateId = template.Id, ParentId = i3, Name = "Doors & Windows", Code = "3.2", ShortCode = "DRW", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 15 },
            new() { Id = i4, TemplateId = template.Id, ParentId = null, Name = "Services & Amenities", Code = "4", ShortCode = "SVC", Level = 0, SortOrder = 4, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 50, TypicalWeight = 25 },
            new() { Id = i4a, TemplateId = template.Id, ParentId = i4, Name = "Electrical & Plumbing", Code = "4.1", ShortCode = "ELP", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 25, TypicalWeight = 12 },
            new() { Id = i4b, TemplateId = template.Id, ParentId = i4, Name = "HVAC & Fire Protection", Code = "4.2", ShortCode = "HFP", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 8 },
            new() { Id = i4c, TemplateId = template.Id, ParentId = i4, Name = "Landscaping & External", Code = "4.3", ShortCode = "LND", Level = 1, SortOrder = 3, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 10, TypicalWeight = 5 }
        };

        return template;
    }

    private static WbsTemplate CreateCommercialComplexTemplate()
    {
        var template = new WbsTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Commercial Complex",
            Category = "Building Construction",
            Industry = "Construction",
            Description = "WBS for commercial building projects including retail spaces, offices, common areas, and parking",
            IsStandard = true,
            Version = 1,
            Tags = "[\"commercial\", \"building\", \"retail\", \"office\", \"mall\"]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var itemId = Guid.NewGuid;
        var i1 = itemId(); var i1a = itemId(); var i1b = itemId();
        var i2 = itemId(); var i2a = itemId(); var i2b = itemId(); var i2c = itemId();
        var i3 = itemId(); var i3a = itemId(); var i3b = itemId(); var i3c = itemId();
        var i4 = itemId(); var i4a = itemId(); var i4b = itemId();

        template.Items = new List<WbsTemplateItem>
        {
            new() { Id = i1, TemplateId = template.Id, ParentId = null, Name = "Substructure", Code = "1", ShortCode = "SUB", Level = 0, SortOrder = 1, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 35, TypicalWeight = 15 },
            new() { Id = i1a, TemplateId = template.Id, ParentId = i1, Name = "Deep Foundations", Code = "1.1", ShortCode = "DPF", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 8 },
            new() { Id = i1b, TemplateId = template.Id, ParentId = i1, Name = "Basement & Retaining Walls", Code = "1.2", ShortCode = "BSR", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 7 },
            new() { Id = i2, TemplateId = template.Id, ParentId = null, Name = "Main Building", Code = "2", ShortCode = "BLD", Level = 0, SortOrder = 2, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 100, TypicalWeight = 40 },
            new() { Id = i2a, TemplateId = template.Id, ParentId = i2, Name = "Structural Steel & Concrete", Code = "2.1", ShortCode = "SSC", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 50, TypicalWeight = 20 },
            new() { Id = i2b, TemplateId = template.Id, ParentId = i2, Name = "Facade & Glazing", Code = "2.2", ShortCode = "FAC", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 12 },
            new() { Id = i2c, TemplateId = template.Id, ParentId = i2, Name = "Roof & Waterproofing", Code = "2.3", ShortCode = "RWF", Level = 1, SortOrder = 3, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 8 },
            new() { Id = i3, TemplateId = template.Id, ParentId = null, Name = "Interior Fit-Out", Code = "3", ShortCode = "FIT", Level = 0, SortOrder = 3, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 70, TypicalWeight = 25 },
            new() { Id = i3a, TemplateId = template.Id, ParentId = i3, Name = "Retail & Common Areas", Code = "3.1", ShortCode = "RCA", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 10 },
            new() { Id = i3b, TemplateId = template.Id, ParentId = i3, Name = "Office Spaces", Code = "3.2", ShortCode = "OFC", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 25, TypicalWeight = 9 },
            new() { Id = i3c, TemplateId = template.Id, ParentId = i3, Name = "Food Court & Restrooms", Code = "3.3", ShortCode = "FCR", Level = 1, SortOrder = 3, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 6 },
            new() { Id = i4, TemplateId = template.Id, ParentId = null, Name = "Building Services", Code = "4", ShortCode = "SVC", Level = 0, SortOrder = 4, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 60, TypicalWeight = 20 },
            new() { Id = i4a, TemplateId = template.Id, ParentId = i4, Name = "Electrical & Data", Code = "4.1", ShortCode = "ELD", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 10 },
            new() { Id = i4b, TemplateId = template.Id, ParentId = i4, Name = "HVAC & Fire Safety", Code = "4.2", ShortCode = "HFS", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 10 }
        };

        return template;
    }

    private static WbsTemplate CreateRoadsAndBridgesTemplate()
    {
        var template = new WbsTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Roads & Bridges",
            Category = "Infrastructure",
            Industry = "Civil Engineering",
            Description = "WBS for road and bridge construction including earthworks, pavement, structures, and traffic systems",
            IsStandard = true,
            Version = 1,
            Tags = "[\"road\", \"bridge\", \"highway\", \"infrastructure\", \"transportation\"]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var itemId = Guid.NewGuid;
        var i1 = itemId(); var i1a = itemId(); var i1b = itemId();
        var i2 = itemId(); var i2a = itemId(); var i2b = itemId();
        var i3 = itemId(); var i3a = itemId(); var i3b = itemId(); var i3c = itemId();
        var i4 = itemId(); var i4a = itemId(); var i4b = itemId();

        template.Items = new List<WbsTemplateItem>
        {
            new() { Id = i1, TemplateId = template.Id, ParentId = null, Name = "Earthworks & Alignment", Code = "1", ShortCode = "ERT", Level = 0, SortOrder = 1, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 50, TypicalWeight = 20 },
            new() { Id = i1a, TemplateId = template.Id, ParentId = i1, Name = "Clearing & Grubbing", Code = "1.1", ShortCode = "CLG", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 5 },
            new() { Id = i1b, TemplateId = template.Id, ParentId = i1, Name = "Cut & Fill Operations", Code = "1.2", ShortCode = "CTF", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 35, TypicalWeight = 15 },
            new() { Id = i2, TemplateId = template.Id, ParentId = null, Name = "Pavement Structure", Code = "2", ShortCode = "PAV", Level = 0, SortOrder = 2, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 60, TypicalWeight = 30 },
            new() { Id = i2a, TemplateId = template.Id, ParentId = i2, Name = "Sub-base & Base Course", Code = "2.1", ShortCode = "SBC", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 25, TypicalWeight = 12 },
            new() { Id = i2b, TemplateId = template.Id, ParentId = i2, Name = "Asphalt/Concrete Wearing Course", Code = "2.2", ShortCode = "WCR", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 35, TypicalWeight = 18 },
            new() { Id = i3, TemplateId = template.Id, ParentId = null, Name = "Bridge Structures", Code = "3", ShortCode = "BRG", Level = 0, SortOrder = 3, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 80, TypicalWeight = 30 },
            new() { Id = i3a, TemplateId = template.Id, ParentId = i3, Name = "Foundations & Substructure", Code = "3.1", ShortCode = "FND", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 12 },
            new() { Id = i3b, TemplateId = template.Id, ParentId = i3, Name = "Superstructure & Deck", Code = "3.2", ShortCode = "SPD", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 35, TypicalWeight = 13 },
            new() { Id = i3c, TemplateId = template.Id, ParentId = i3, Name = "Bridge Railings & Bearings", Code = "3.3", ShortCode = "BRB", Level = 1, SortOrder = 3, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 5 },
            new() { Id = i4, TemplateId = template.Id, ParentId = null, Name = "Traffic & Safety Systems", Code = "4", ShortCode = "TFS", Level = 0, SortOrder = 4, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 30, TypicalWeight = 20 },
            new() { Id = i4a, TemplateId = template.Id, ParentId = i4, Name = "Signage & Markings", Code = "4.1", ShortCode = "SGN", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 10 },
            new() { Id = i4b, TemplateId = template.Id, ParentId = i4, Name = "Lighting & Barriers", Code = "4.2", ShortCode = "LGB", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 10 }
        };

        return template;
    }

    private static WbsTemplate CreateWaterTreatmentTemplate()
    {
        var template = new WbsTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Water Treatment Plant",
            Category = "Infrastructure",
            Industry = "Civil Engineering",
            Description = "WBS for water treatment plant construction including intake, treatment processes, storage, and distribution",
            IsStandard = true,
            Version = 1,
            Tags = "[\"water\", \"treatment\", \"plant\", \"infrastructure\", \"utility\"]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var itemId = Guid.NewGuid;
        var i1 = itemId(); var i1a = itemId(); var i1b = itemId();
        var i2 = itemId(); var i2a = itemId(); var i2b = itemId();
        var i3 = itemId(); var i3a = itemId(); var i3b = itemId();
        var i4 = itemId(); var i4a = itemId(); var i4b = itemId();

        template.Items = new List<WbsTemplateItem>
        {
            new() { Id = i1, TemplateId = template.Id, ParentId = null, Name = "Intake & Raw Water", Code = "1", ShortCode = "INT", Level = 0, SortOrder = 1, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 40, TypicalWeight = 15 },
            new() { Id = i1a, TemplateId = template.Id, ParentId = i1, Name = "Intake Structure", Code = "1.1", ShortCode = "IST", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 8 },
            new() { Id = i1b, TemplateId = template.Id, ParentId = i1, Name = "Raw Water Pumping", Code = "1.2", ShortCode = "RWP", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 7 },
            new() { Id = i2, TemplateId = template.Id, ParentId = null, Name = "Treatment Processes", Code = "2", ShortCode = "TRT", Level = 0, SortOrder = 2, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 80, TypicalWeight = 40 },
            new() { Id = i2a, TemplateId = template.Id, ParentId = i2, Name = "Coagulation & Flocculation", Code = "2.1", ShortCode = "CFL", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 10 },
            new() { Id = i2b, TemplateId = template.Id, ParentId = i2, Name = "Sedimentation & Filtration", Code = "2.2", ShortCode = "SFL", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 15 },
            new() { Id = i3, TemplateId = template.Id, ParentId = null, Name = "Chemical & Disinfection", Code = "3", ShortCode = "CHM", Level = 0, SortOrder = 3, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 40, TypicalWeight = 20 },
            new() { Id = i3a, TemplateId = template.Id, ParentId = i3, Name = "Chemical Dosing Systems", Code = "3.1", ShortCode = "DOS", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 10 },
            new() { Id = i3b, TemplateId = template.Id, ParentId = i3, Name = "Disinfection & UV System", Code = "3.2", ShortCode = "DIS", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 10 },
            new() { Id = i4, TemplateId = template.Id, ParentId = null, Name = "Storage & Distribution", Code = "4", ShortCode = "STD", Level = 0, SortOrder = 4, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 50, TypicalWeight = 25 },
            new() { Id = i4a, TemplateId = template.Id, ParentId = i4, Name = "Clear Water Tank", Code = "4.1", ShortCode = "CWT", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 25, TypicalWeight = 12 },
            new() { Id = i4b, TemplateId = template.Id, ParentId = i4, Name = "Distribution Pumping", Code = "4.2", ShortCode = "DPM", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 25, TypicalWeight = 13 }
        };

        return template;
    }

    private static WbsTemplate CreatePowerPlantTemplate()
    {
        var template = new WbsTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Power Plant",
            Category = "Industrial",
            Industry = "Energy",
            Description = "WBS for power plant construction including generation equipment, electrical systems, cooling, and fuel handling",
            IsStandard = true,
            Version = 1,
            Tags = "[\"power\", \"plant\", \"energy\", \"generation\", \"electrical\"]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var itemId = Guid.NewGuid;
        var i1 = itemId(); var i1a = itemId(); var i1b = itemId();
        var i2 = itemId(); var i2a = itemId(); var i2b = itemId(); var i2c = itemId();
        var i3 = itemId(); var i3a = itemId(); var i3b = itemId();
        var i4 = itemId(); var i4a = itemId(); var i4b = itemId();

        template.Items = new List<WbsTemplateItem>
        {
            new() { Id = i1, TemplateId = template.Id, ParentId = null, Name = "Civil & Structural", Code = "1", ShortCode = "CIV", Level = 0, SortOrder = 1, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 60, TypicalWeight = 20 },
            new() { Id = i1a, TemplateId = template.Id, ParentId = i1, Name = "Turbine Hall Foundation", Code = "1.1", ShortCode = "TFN", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 10 },
            new() { Id = i1b, TemplateId = template.Id, ParentId = i1, Name = "Cooling Tower Foundation", Code = "1.2", ShortCode = "CTF", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 10 },
            new() { Id = i2, TemplateId = template.Id, ParentId = null, Name = "Generation Equipment", Code = "2", ShortCode = "GEN", Level = 0, SortOrder = 2, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 90, TypicalWeight = 35 },
            new() { Id = i2a, TemplateId = template.Id, ParentId = i2, Name = "Turbine & Generator", Code = "2.1", ShortCode = "TGN", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 40, TypicalWeight = 15 },
            new() { Id = i2b, TemplateId = template.Id, ParentId = i2, Name = "Boiler & Heat Recovery", Code = "2.2", ShortCode = "BHR", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 12 },
            new() { Id = i2c, TemplateId = template.Id, ParentId = i2, Name = "Transformers & Switchgear", Code = "2.3", ShortCode = "TRS", Level = 1, SortOrder = 3, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 8 },
            new() { Id = i3, TemplateId = template.Id, ParentId = null, Name = "Cooling & Auxiliary Systems", Code = "3", ShortCode = "CLG", Level = 0, SortOrder = 3, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 50, TypicalWeight = 25 },
            new() { Id = i3a, TemplateId = template.Id, ParentId = i3, Name = "Cooling Water System", Code = "3.1", ShortCode = "CWS", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 15 },
            new() { Id = i3b, TemplateId = template.Id, ParentId = i3, Name = "Compressed Air & Aux", Code = "3.2", ShortCode = "CAX", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 10 },
            new() { Id = i4, TemplateId = template.Id, ParentId = null, Name = "Fuel Handling & Storage", Code = "4", ShortCode = "FHS", Level = 0, SortOrder = 4, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 40, TypicalWeight = 20 },
            new() { Id = i4a, TemplateId = template.Id, ParentId = i4, Name = "Fuel Storage Tanks", Code = "4.1", ShortCode = "FST", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 10 },
            new() { Id = i4b, TemplateId = template.Id, ParentId = i4, Name = "Fuel Transfer System", Code = "4.2", ShortCode = "FTS", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 10 }
        };

        return template;
    }

    private static WbsTemplate CreateTelecommunicationsTemplate()
    {
        var template = new WbsTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Telecommunications Network",
            Category = "Infrastructure",
            Industry = "Telecommunications",
            Description = "WBS for telecom network deployment including fiber optics, towers, equipment, and commissioning",
            IsStandard = true,
            Version = 1,
            Tags = "[\"telecom\", \"fiber\", \"network\", \"tower\", \"communication\"]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var itemId = Guid.NewGuid;
        var i1 = itemId(); var i1a = itemId(); var i1b = itemId();
        var i2 = itemId(); var i2a = itemId(); var i2b = itemId(); var i2c = itemId();
        var i3 = itemId(); var i3a = itemId(); var i3b = itemId();
        var i4 = itemId(); var i4a = itemId(); var i4b = itemId();

        template.Items = new List<WbsTemplateItem>
        {
            new() { Id = i1, TemplateId = template.Id, ParentId = null, Name = "Network Planning", Code = "1", ShortCode = "PLN", Level = 0, SortOrder = 1, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 30, TypicalWeight = 10 },
            new() { Id = i1a, TemplateId = template.Id, ParentId = i1, Name = "Site Survey & Selection", Code = "1.1", ShortCode = "SSS", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 5 },
            new() { Id = i1b, TemplateId = template.Id, ParentId = i1, Name = "Permits & Approvals", Code = "1.2", ShortCode = "PRM", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 5 },
            new() { Id = i2, TemplateId = template.Id, ParentId = null, Name = "Infrastructure Deployment", Code = "2", ShortCode = "DEP", Level = 0, SortOrder = 2, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 80, TypicalWeight = 40 },
            new() { Id = i2a, TemplateId = template.Id, ParentId = i2, Name = "Tower & Mast Construction", Code = "2.1", ShortCode = "TWR", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 15 },
            new() { Id = i2b, TemplateId = template.Id, ParentId = i2, Name = "Fiber Optic Cabling", Code = "2.2", ShortCode = "FIB", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 15 },
            new() { Id = i2c, TemplateId = template.Id, ParentId = i2, Name = "Equipment Shelter", Code = "2.3", ShortCode = "SHL", Level = 1, SortOrder = 3, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 10 },
            new() { Id = i3, TemplateId = template.Id, ParentId = null, Name = "Equipment Installation", Code = "3", ShortCode = "EQP", Level = 0, SortOrder = 3, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 50, TypicalWeight = 30 },
            new() { Id = i3a, TemplateId = template.Id, ParentId = i3, Name = "Radio & Antenna Systems", Code = "3.1", ShortCode = "RAS", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 12 },
            new() { Id = i3b, TemplateId = template.Id, ParentId = i3, Name = "Switching & Transmission", Code = "3.2", ShortCode = "SWT", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 18 },
            new() { Id = i4, TemplateId = template.Id, ParentId = null, Name = "Commissioning & Testing", Code = "4", ShortCode = "COT", Level = 0, SortOrder = 4, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 30, TypicalWeight = 20 },
            new() { Id = i4a, TemplateId = template.Id, ParentId = i4, Name = "Network Integration", Code = "4.1", ShortCode = "INT", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 10 },
            new() { Id = i4b, TemplateId = template.Id, ParentId = i4, Name = "Acceptance Testing", Code = "4.2", ShortCode = "ACP", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 10 }
        };

        return template;
    }

    private static WbsTemplate CreateOilAndGasTemplate()
    {
        var template = new WbsTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Oil & Gas Project",
            Category = "Oil & Gas",
            Industry = "Energy",
            Description = "WBS for oil and gas projects including well pads, pipelines, processing, and safety systems",
            IsStandard = true,
            Version = 1,
            Tags = "[\"oil\", \"gas\", \"energy\", \"pipeline\", \"offshore\"]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var itemId = Guid.NewGuid;
        var i1 = itemId(); var i1a = itemId(); var i1b = itemId();
        var i2 = itemId(); var i2a = itemId(); var i2b = itemId();
        var i3 = itemId(); var i3a = itemId(); var i3b = itemId();

        template.Items = new List<WbsTemplateItem>
        {
            new() { Id = i1, TemplateId = template.Id, ParentId = null, Name = "Well Pad Construction", Code = "1", ShortCode = "WEL", Level = 0, SortOrder = 1, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 45, TypicalWeight = 25 },
            new() { Id = i1a, TemplateId = template.Id, ParentId = i1, Name = "Site Preparation", Code = "1.1", ShortCode = "PRE", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 8 },
            new() { Id = i1b, TemplateId = template.Id, ParentId = i1, Name = "Pad Foundation", Code = "1.2", ShortCode = "PAD", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 17 },
            new() { Id = i2, TemplateId = template.Id, ParentId = null, Name = "Pipeline Network", Code = "2", ShortCode = "PIP", Level = 0, SortOrder = 2, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 50, TypicalWeight = 35 },
            new() { Id = i2a, TemplateId = template.Id, ParentId = i2, Name = "Main Pipeline", Code = "2.1", ShortCode = "MNP", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 20 },
            new() { Id = i2b, TemplateId = template.Id, ParentId = i2, Name = "Distribution Lines", Code = "2.2", ShortCode = "DST", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 15 },
            new() { Id = i3, TemplateId = template.Id, ParentId = null, Name = "Processing Facilities", Code = "3", ShortCode = "PRC", Level = 0, SortOrder = 3, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 60, TypicalWeight = 30 },
            new() { Id = i3a, TemplateId = template.Id, ParentId = i3, Name = "Separation & Treatment", Code = "3.1", ShortCode = "SEP", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 35, TypicalWeight = 18 },
            new() { Id = i3b, TemplateId = template.Id, ParentId = i3, Name = "Safety Systems", Code = "3.2", ShortCode = "SAF", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 25, TypicalWeight = 12 }
        };

        return template;
    }

    private static void AddStandardMilestones(WbsTemplate template)
    {
        var items = template.Items as List<WbsTemplateItem> ?? template.Items.ToList();
        template.Items = items;

        var existingRoots = items.Where(i => i.ParentId is null).ToList();
        var nextSort = existingRoots.Count > 0 ? existingRoots.Max(i => i.SortOrder) + 1 : 1;
        var nextCode = existingRoots.Count + 1;

        var itemId = Guid.NewGuid;
        var engId = itemId(); var eng1 = itemId(); var eng2 = itemId();
        var proId = itemId(); var pro1 = itemId(); var pro2 = itemId(); var pro3 = itemId(); var pro4 = itemId();
        var conId = itemId(); var con1 = itemId(); var con2 = itemId();
        var tstId = itemId(); var tst1 = itemId(); var tst2 = itemId();
        var hanId = itemId(); var han1 = itemId(); var han2 = itemId();
        var asbId = itemId();

        items.AddRange(new[]
        {
            // Engineering
            new WbsTemplateItem { Id = engId, TemplateId = template.Id, ParentId = null, Name = "Engineering", Code = $"{nextCode++}", ShortCode = "ENG", Level = 0, SortOrder = nextSort++, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 30, TypicalWeight = 10 },
            new WbsTemplateItem { Id = eng1, TemplateId = template.Id, ParentId = engId, Name = "Shop Drawings & Design", Code = $"{nextCode - 1}.1", ShortCode = "SDD", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 6 },
            new WbsTemplateItem { Id = eng2, TemplateId = template.Id, ParentId = engId, Name = "Approvals & Submittals", Code = $"{nextCode - 1}.2", ShortCode = "APS", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 10, TypicalWeight = 4 },

            // Procurement
            new WbsTemplateItem { Id = proId, TemplateId = template.Id, ParentId = null, Name = "Procurement", Code = $"{nextCode++}", ShortCode = "PRO", Level = 0, SortOrder = nextSort++, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 60, TypicalWeight = 15 },
            new WbsTemplateItem { Id = pro1, TemplateId = template.Id, ParentId = proId, Name = "Materials & Equipment", Code = $"{nextCode - 1}.1", ShortCode = "MAT", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 5 },
            new WbsTemplateItem { Id = pro2, TemplateId = template.Id, ParentId = proId, Name = "Purchase Orders", Code = $"{nextCode - 1}.2", ShortCode = "PO", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 10, TypicalWeight = 3 },
            new WbsTemplateItem { Id = pro3, TemplateId = template.Id, ParentId = proId, Name = "Delivery & Fabrication", Code = $"{nextCode - 1}.3", ShortCode = "DLV", Level = 1, SortOrder = 3, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 20, TypicalWeight = 5 },
            new WbsTemplateItem { Id = pro4, TemplateId = template.Id, ParentId = proId, Name = "Submittal & Approval", Code = $"{nextCode - 1}.4", ShortCode = "SAP", Level = 1, SortOrder = 4, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 10, TypicalWeight = 2 },

            // Construction (generic)
            new WbsTemplateItem { Id = conId, TemplateId = template.Id, ParentId = null, Name = "Construction", Code = $"{nextCode++}", ShortCode = "CON", Level = 0, SortOrder = nextSort++, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 90, TypicalWeight = 40 },
            new WbsTemplateItem { Id = con1, TemplateId = template.Id, ParentId = conId, Name = "Execution", Code = $"{nextCode - 1}.1", ShortCode = "EXE", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 60, TypicalWeight = 25 },
            new WbsTemplateItem { Id = con2, TemplateId = template.Id, ParentId = conId, Name = "Quality Control", Code = $"{nextCode - 1}.2", ShortCode = "QC", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 30, TypicalWeight = 15 },

            // Testing & Commissioning
            new WbsTemplateItem { Id = tstId, TemplateId = template.Id, ParentId = null, Name = "Testing & Commissioning", Code = $"{nextCode++}", ShortCode = "TST", Level = 0, SortOrder = nextSort++, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 30, TypicalWeight = 10 },
            new WbsTemplateItem { Id = tst1, TemplateId = template.Id, ParentId = tstId, Name = "System Testing", Code = $"{nextCode - 1}.1", ShortCode = "SYS", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 5 },
            new WbsTemplateItem { Id = tst2, TemplateId = template.Id, ParentId = tstId, Name = "Commissioning", Code = $"{nextCode - 1}.2", ShortCode = "COM", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 15, TypicalWeight = 5 },

            // Handing Over
            new WbsTemplateItem { Id = hanId, TemplateId = template.Id, ParentId = null, Name = "Handing Over", Code = $"{nextCode++}", ShortCode = "HND", Level = 0, SortOrder = nextSort++, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 15, TypicalWeight = 5 },
            new WbsTemplateItem { Id = han1, TemplateId = template.Id, ParentId = hanId, Name = "Documentation", Code = $"{nextCode - 1}.1", ShortCode = "DOC", Level = 1, SortOrder = 1, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 10, TypicalWeight = 3 },
            new WbsTemplateItem { Id = han2, TemplateId = template.Id, ParentId = hanId, Name = "Client Training", Code = $"{nextCode - 1}.2", ShortCode = "TRN", Level = 1, SortOrder = 2, WbsLevel = WbsLevelType.ControlAccount, DefaultDurationDays = 5, TypicalWeight = 2 },

            // As Built
            new WbsTemplateItem { Id = asbId, TemplateId = template.Id, ParentId = null, Name = "As Built Documentation", Code = $"{nextCode++}", ShortCode = "ASB", Level = 0, SortOrder = nextSort++, WbsLevel = WbsLevelType.Summary, DefaultDurationDays = 10, TypicalWeight = 5 }
        });
    }
}
