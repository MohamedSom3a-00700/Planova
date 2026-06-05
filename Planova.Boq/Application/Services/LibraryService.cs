using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Application.Services;

public class LibraryService
{
    private readonly IBoqLibraryRepository _repository;

    public LibraryService(IBoqLibraryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<LibraryDto>> GetAllAsync(CancellationToken ct)
    {
        var libraries = await _repository.GetAllAsync(ct);
        return libraries.Select(l => new LibraryDto(
            l.Id, l.Name, l.Description, l.LibraryType, l.CreatedAt,
            l.Items?.Count ?? 0
        )).ToList();
    }

    public async Task<LibraryDto> CreateAsync(string name, string? description, LibraryType libraryType, CancellationToken ct)
    {
        var library = new BoqLibrary
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            LibraryType = libraryType,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            CreatedBy = "system",
        };

        var created = await _repository.AddAsync(library, ct);
        return new LibraryDto(created.Id, created.Name, created.Description, created.LibraryType, created.CreatedAt, 0);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await _repository.DeleteAsync(id, ct);
    }

    public async Task<LibraryItemDto> AddItemAsync(Guid libraryId, string code, string description,
        string unit, decimal defaultRate, string? category, string? tags, CancellationToken ct)
    {
        var item = new BoqLibraryItem
        {
            Id = Guid.NewGuid(),
            LibraryId = libraryId,
            Code = code,
            Description = description,
            Unit = unit,
            DefaultRate = defaultRate,
            Category = category,
            Tags = tags,
        };

        return new LibraryItemDto(item.Id, item.LibraryId, item.Code, item.Description,
            item.Unit, item.DefaultRate, item.Category, item.Tags);
    }
}
