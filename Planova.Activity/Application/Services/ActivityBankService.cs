using System.Text.Json;
using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Enums;
using Planova.Activity.Domain.Interfaces;

using ActivityBankEntity = Planova.Activity.Domain.Entities.ActivityBank;
using ActivityBankItemEntity = Planova.Activity.Domain.Entities.ActivityBankItem;
using ActivityBankItemRelationshipEntity = Planova.Activity.Domain.Entities.ActivityBankItemRelationship;

namespace Planova.Activity.Application.Services;

public class ActivityBankService : IActivityBankService
{
    private readonly IActivityBankRepository _bankRepository;
    private readonly IActivityBankItemRepository _itemRepository;
    private readonly IActivityBankItemRelationshipRepository _relationshipRepository;
    private readonly IActivityRepository _activityRepository;

    public ActivityBankService(
        IActivityBankRepository bankRepository,
        IActivityBankItemRepository itemRepository,
        IActivityBankItemRelationshipRepository relationshipRepository,
        IActivityRepository activityRepository)
    {
        _bankRepository = bankRepository;
        _itemRepository = itemRepository;
        _relationshipRepository = relationshipRepository;
        _activityRepository = activityRepository;
    }

    public async Task<ActivityBankDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var bank = await _bankRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Activity bank entry {id} not found");
        return MapToDto(bank);
    }

    public async Task<List<ActivityBankDto>> BrowseAsync(string? category = null, string? search = null, CancellationToken ct = default)
    {
        List<ActivityBankEntity> banks;

        if (!string.IsNullOrWhiteSpace(search))
            banks = await _bankRepository.SearchAsync(search, ct);
        else if (!string.IsNullOrWhiteSpace(category))
            banks = await _bankRepository.GetByCategoryAsync(category, ct);
        else
        {
            var categories = await _bankRepository.GetCategoriesAsync(ct);
            banks = [];
            foreach (var cat in categories)
            {
                banks.AddRange(await _bankRepository.GetByCategoryAsync(cat, ct));
            }
        }

        return banks.Select(MapToDto).ToList();
    }

    public async Task<List<string>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await _bankRepository.GetCategoriesAsync(ct);
    }

    public async Task<ActivityBankDto> CreateCustomAsync(CreateBankEntryRequest request, CancellationToken ct = default)
    {
        var bank = new ActivityBankEntity
        {
            Id = Guid.NewGuid(),
            Category = request.Category,
            Subcategory = request.Subcategory,
            Code = $"{request.Category.ToUpperInvariant().Substring(0, 4)}-CUSTOM-001",
            Name = request.Name,
            Description = request.Description,
            IsStandard = false,
            Version = 1,
            Tags = JsonSerializer.Serialize(request.Tags),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _bankRepository.AddAsync(bank, ct);

        foreach (var item in request.Items)
        {
            var bankItem = new ActivityBankItemEntity
            {
                Id = Guid.NewGuid(),
                BankId = bank.Id,
                ParentId = item.ParentId,
                Code = item.Code,
                Name = item.Name,
                Description = item.Description,
                Level = item.Level,
                SortOrder = item.SortOrder,
                DefaultDuration = item.DefaultDuration,
                DefaultActivityType = Enum.Parse<ActivityType>(item.DefaultActivityType),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _itemRepository.AddRangeAsync([bankItem], ct);
        }

        foreach (var rel in request.Relationships)
        {
            var bankRel = new ActivityBankItemRelationshipEntity
            {
                Id = Guid.NewGuid(),
                BankId = bank.Id,
                PredecessorItemId = rel.PredecessorItemId,
                SuccessorItemId = rel.SuccessorItemId,
                Type = Enum.Parse<RelationshipType>(rel.Type),
                DefaultLagDays = rel.DefaultLagDays
            };
            await _relationshipRepository.AddRangeAsync([bankRel], ct);
        }

        return MapToDto(bank);
    }

    public async Task<ActivityBankDto> UpdateCustomAsync(UpdateBankEntryRequest request, CancellationToken ct = default)
    {
        var bank = await _bankRepository.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Activity bank entry {request.Id} not found");

        if (bank.IsStandard)
            throw new InvalidOperationException("Cannot update standard bank entries");

        if (request.Name is not null) bank.Name = request.Name;
        if (request.Description is not null) bank.Description = request.Description;
        if (request.Tags is not null) bank.Tags = JsonSerializer.Serialize(request.Tags);
        bank.Version++;
        bank.UpdatedAt = DateTime.UtcNow;

        await _bankRepository.UpdateAsync(bank, ct);

        if (request.Items is not null)
        {
            await _itemRepository.DeleteByBankIdAsync(bank.Id, ct);
            foreach (var item in request.Items)
            {
                var bankItem = new ActivityBankItemEntity
                {
                    Id = Guid.NewGuid(),
                    BankId = bank.Id,
                    ParentId = item.ParentId,
                    Code = item.Code,
                    Name = item.Name,
                    Level = item.Level,
                    SortOrder = item.SortOrder,
                    DefaultDuration = item.DefaultDuration,
                    DefaultActivityType = Enum.Parse<ActivityType>(item.DefaultActivityType),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _itemRepository.AddRangeAsync([bankItem], ct);
            }
        }

        if (request.Relationships is not null)
        {
            await _relationshipRepository.DeleteByBankIdAsync(bank.Id, ct);
            foreach (var rel in request.Relationships)
            {
                var bankRel = new ActivityBankItemRelationshipEntity
                {
                    Id = Guid.NewGuid(),
                    BankId = bank.Id,
                    PredecessorItemId = rel.PredecessorItemId,
                    SuccessorItemId = rel.SuccessorItemId,
                    Type = Enum.Parse<RelationshipType>(rel.Type),
                    DefaultLagDays = rel.DefaultLagDays
                };
                await _relationshipRepository.AddRangeAsync([bankRel], ct);
            }
        }

        return MapToDto(bank);
    }

    public async Task DeleteCustomAsync(Guid id, CancellationToken ct = default)
    {
        var isStandard = await _bankRepository.IsStandardAsync(id, ct);
        if (isStandard)
            throw new InvalidOperationException("Cannot delete standard bank entries");

        await _bankRepository.DeleteAsync(id, ct);
    }

    public async Task SeedIfEmptyAsync(CancellationToken ct = default)
    {
        var categories = await _bankRepository.GetCategoriesAsync(ct);
        if (categories.Count > 0) return;

        var assembly = typeof(ActivityBankService).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("activity-bank-seed.json"));

        List<SeedEntry> entries;

        if (resourceName is not null)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            var json = await reader.ReadToEndAsync(ct);
            entries = JsonSerializer.Deserialize<List<SeedEntry>>(json) ?? [];
        }
        else
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Planova.Activity", "Application", "Data", "activity-bank-seed.json");
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath, ct);
                entries = JsonSerializer.Deserialize<List<SeedEntry>>(json) ?? [];
            }
            else
            {
                return;
            }
        }

        foreach (var entry in entries)
        {
            var bank = new ActivityBankEntity
            {
                Id = Guid.NewGuid(),
                Category = entry.Category,
                Subcategory = entry.Subcategory,
                Code = entry.Code,
                Name = entry.Name,
                Description = entry.Description,
                IsStandard = true,
                Version = 1,
                Tags = "[]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _bankRepository.AddAsync(bank, ct);

            var codeToIdMap = new Dictionary<string, Guid>();

            foreach (var item in entry.Items)
            {
                var bankItem = new ActivityBankItemEntity
                {
                    Id = Guid.NewGuid(),
                    BankId = bank.Id,
                    Code = item.Code,
                    Name = item.Name,
                    Level = item.Level,
                    SortOrder = item.SortOrder,
                    DefaultDuration = item.DefaultDuration,
                    DefaultActivityType = Enum.Parse<ActivityType>(item.DefaultActivityType),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                codeToIdMap[item.Code] = bankItem.Id;
                await _itemRepository.AddRangeAsync([bankItem], ct);
            }

            foreach (var rel in entry.Relationships)
            {
                if (!codeToIdMap.TryGetValue(rel.PredecessorItemCode, out var predId) ||
                    !codeToIdMap.TryGetValue(rel.SuccessorItemCode, out var succId))
                    continue;

                var bankRel = new ActivityBankItemRelationshipEntity
                {
                    Id = Guid.NewGuid(),
                    BankId = bank.Id,
                    PredecessorItemId = predId,
                    SuccessorItemId = succId,
                    Type = Enum.Parse<RelationshipType>(rel.Type),
                    DefaultLagDays = rel.DefaultLagDays
                };
                await _relationshipRepository.AddRangeAsync([bankRel], ct);
            }
        }
    }

    public async Task<ActivityBankDto> SaveActivitiesAsBankEntryAsync(List<Guid> activityIds, string category, string name, string? description = null, CancellationToken ct = default)
    {
        var activities = new List<Planova.Activity.Domain.Entities.Activity>();
        foreach (var id in activityIds)
        {
            var activity = await _activityRepository.GetByIdAsync(id, ct);
            if (activity is not null) activities.Add(activity);
        }

        var bank = new ActivityBankEntity
        {
            Id = Guid.NewGuid(),
            Category = category,
            Code = $"{category.ToUpperInvariant().Substring(0, Math.Min(4, category.Length))}-CUSTOM-{DateTime.UtcNow:yyyyMMdd}",
            Name = name,
            Description = description,
            IsStandard = false,
            Version = 1,
            Tags = "[]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _bankRepository.AddAsync(bank, ct);

        foreach (var activity in activities.OrderBy(a => a.SortOrder))
        {
            var bankItem = new ActivityBankItemEntity
            {
                Id = Guid.NewGuid(),
                BankId = bank.Id,
                Code = activity.Code,
                Name = activity.Name,
                Description = activity.Description,
                Level = 0,
                SortOrder = activity.SortOrder,
                DefaultDuration = activity.Duration ?? 5,
                DefaultActivityType = activity.ActivityType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _itemRepository.AddRangeAsync([bankItem], ct);
        }

        return MapToDto(bank);
    }

    private class SeedEntry
    {
        public string Category { get; set; } = string.Empty;
        public string? Subcategory { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<SeedItem> Items { get; set; } = [];
        public List<SeedRelationship> Relationships { get; set; } = [];
    }

    private class SeedItem
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int SortOrder { get; set; }
        public int DefaultDuration { get; set; }
        public string DefaultActivityType { get; set; } = "Task";
    }

    private class SeedRelationship
    {
        public string PredecessorItemCode { get; set; } = string.Empty;
        public string SuccessorItemCode { get; set; } = string.Empty;
        public string Type { get; set; } = "FS";
        public int DefaultLagDays { get; set; }
    }

    private static ActivityBankDto MapToDto(ActivityBankEntity bank)
    {
        var tags = new List<string>();
        try { tags = JsonSerializer.Deserialize<List<string>>(bank.Tags) ?? []; } catch { }

        return new ActivityBankDto
        {
            Id = bank.Id,
            Category = bank.Category,
            Subcategory = bank.Subcategory,
            Code = bank.Code,
            Name = bank.Name,
            Description = bank.Description,
            IsStandard = bank.IsStandard,
            Version = bank.Version,
            Tags = tags,
            Items = bank.Items.Select(i => new ActivityBankItemDto
            {
                Id = i.Id,
                BankId = i.BankId,
                ParentId = i.ParentId,
                Code = i.Code,
                Name = i.Name,
                Description = i.Description,
                Level = i.Level,
                SortOrder = i.SortOrder,
                DefaultDuration = i.DefaultDuration,
                DefaultActivityType = i.DefaultActivityType.ToString()
            }).ToList(),
            Relationships = bank.Relationships.Select(r => new ActivityBankItemRelationshipDto
            {
                Id = r.Id,
                BankId = r.BankId,
                PredecessorItemId = r.PredecessorItemId,
                SuccessorItemId = r.SuccessorItemId,
                Type = r.Type.ToString(),
                DefaultLagDays = r.DefaultLagDays
            }).ToList()
        };
    }
}
