using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Application.Services;

public class ClassificationService
{
    private readonly IBoqClassificationRepository _repository;

    public ClassificationService(IBoqClassificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ClassificationDto>> GetTreeAsync(Guid? projectId, CancellationToken ct)
    {
        var all = new List<BoqClassification>();

        if (projectId.HasValue)
        {
            all.AddRange(await _repository.GetByProjectIdAsync(projectId, ct));
        }

        all.AddRange(await _repository.GetGlobalAsync(ct));

        return BuildTree(all, null);
    }

    public async Task<ClassificationDto> CreateAsync(string code, string name, string? description,
        ClassificationScope scope, Guid? projectId, Guid? parentId, CancellationToken ct)
    {
        var entity = new BoqClassification
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description,
            Scope = scope,
            ProjectId = projectId,
            ParentId = parentId,
        };

        var created = await _repository.AddAsync(entity, ct);
        return MapToDto(created, []);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await _repository.DeleteAsync(id, ct);
    }

    private static List<ClassificationDto> BuildTree(IReadOnlyList<BoqClassification> items, Guid? parentId)
    {
        return items
            .Where(i => i.ParentId == parentId)
            .Select(i =>
            {
                var children = BuildTree(items, i.Id);
                return MapToDto(i, children);
            })
            .ToList();
    }

    private static ClassificationDto MapToDto(BoqClassification c, List<ClassificationDto> children) => new(
        c.Id, c.ParentId, c.Code, c.Name, c.Description, c.Scope, c.ProjectId, children
    );
}
