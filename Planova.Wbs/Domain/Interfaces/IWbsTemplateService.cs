namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsTemplateService
{
    Task<Entities.WbsTemplate> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Entities.WbsTemplate>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<Entities.WbsTemplate>> GetByCategoryAsync(string category, CancellationToken ct);
    Task<Entities.WbsTemplate> CreateAsync(string name, string category, string? industry, string? description, CancellationToken ct);
    Task<Entities.WbsTemplate> UpdateAsync(Entities.WbsTemplate template, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<Entities.Wbs> ApplyAsync(Guid templateId, string wbsName, int projectId, CancellationToken ct);
    Task<Entities.WbsTemplate> ImportFromJsonAsync(string json, CancellationToken ct);
    Task<string> ExportToJsonAsync(Guid templateId, CancellationToken ct);
}
