using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Application.Services;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;
using WbsItemEntity = Planova.Wbs.Domain.Entities.WbsItem;
using WbsTemplateEntity = Planova.Wbs.Domain.Entities.WbsTemplate;
using WbsTemplateItemEntity = Planova.Wbs.Domain.Entities.WbsTemplateItem;

public class WbsTemplateService : IWbsTemplateService
{
    private readonly IWbsTemplateRepository _templateRepository;
    private readonly IWbsRepository _wbsRepository;
    private readonly IWbsItemRepository _itemRepository;

    public WbsTemplateService(
        IWbsTemplateRepository templateRepository,
        IWbsRepository wbsRepository,
        IWbsItemRepository itemRepository)
    {
        _templateRepository = templateRepository;
        _wbsRepository = wbsRepository;
        _itemRepository = itemRepository;
    }

    public async Task<WbsTemplateEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _templateRepository.GetByIdAsync(id, ct);
    }

    public async Task<IReadOnlyList<WbsTemplateEntity>> GetAllAsync(CancellationToken ct)
    {
        return await _templateRepository.GetAllAsync(ct);
    }

    public async Task<IReadOnlyList<WbsTemplateEntity>> GetByCategoryAsync(string category, CancellationToken ct)
    {
        return await _templateRepository.GetByCategoryAsync(category, ct);
    }

    public async Task<WbsTemplateEntity> CreateAsync(string name, string category, string? industry, string? description, CancellationToken ct)
    {
        var template = new WbsTemplateEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = category,
            Industry = industry ?? string.Empty,
            Description = description,
            IsStandard = false,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return await _templateRepository.AddAsync(template, ct);
    }

    public async Task<WbsTemplateEntity> UpdateAsync(WbsTemplateEntity template, CancellationToken ct)
    {
        template.UpdatedAt = DateTime.UtcNow;
        return await _templateRepository.UpdateAsync(template, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await _templateRepository.DeleteAsync(id, ct);
    }

    public async Task<WbsEntity> ApplyAsync(Guid templateId, string wbsName, int projectId, CancellationToken ct)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, ct);
        var wbs = new WbsEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = wbsName,
            Source = WbsSource.FromTemplate,
            Status = WbsStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        wbs = await _wbsRepository.AddAsync(wbs, ct);

        await DeepCopyItems(template.Items, wbs.Id, null, ct);

        return wbs;
    }

    public async Task<WbsTemplateEntity> ImportFromJsonAsync(string json, CancellationToken ct)
    {
        var template = System.Text.Json.JsonSerializer.Deserialize<WbsTemplateEntity>(json);
        if (template == null)
            throw new InvalidOperationException("Invalid template JSON");
        template.Id = Guid.NewGuid();
        template.IsStandard = false;
        template.Version = 1;
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;
        return await _templateRepository.AddAsync(template, ct);
    }

    public async Task<string> ExportToJsonAsync(Guid templateId, CancellationToken ct)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, ct);
        return System.Text.Json.JsonSerializer.Serialize(template);
    }

    private async Task DeepCopyItems(IEnumerable<WbsTemplateItemEntity> templateItems, Guid wbsId, Guid? parentId, CancellationToken ct)
    {
        foreach (var templateItem in templateItems.OrderBy(i => i.SortOrder))
        {
            var item = new WbsItemEntity
            {
                Id = Guid.NewGuid(),
                WbsId = wbsId,
                ParentId = parentId,
                Code = templateItem.Code,
                ShortCode = templateItem.ShortCode,
                Name = templateItem.Name,
                Description = templateItem.Description,
                Level = templateItem.Level,
                SortOrder = templateItem.SortOrder,
                WbsLevel = templateItem.WbsLevel,
                Weight = templateItem.TypicalWeight,
                DurationDays = templateItem.DefaultDurationDays,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _itemRepository.AddAsync(item, ct);

            if (templateItem.Children.Count > 0)
            {
                await DeepCopyItems(templateItem.Children, wbsId, item.Id, ct);
            }
        }
    }
}
