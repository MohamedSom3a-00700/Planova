using Planova.Application.Dto;

namespace Planova.Application.Services;

public interface IProjectDocumentService
{
    Task<IEnumerable<ProjectDocumentDto>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<IEnumerable<ProjectDocumentDto>> GetByTypeAsync(int projectId, string documentType, CancellationToken ct = default);
    Task<ProjectDocumentDto> AddAsync(AddProjectDocumentDto dto, CancellationToken ct = default);
    Task DeleteAsync(int documentId, CancellationToken ct = default);
    Task<IEnumerable<ProjectDocumentDto>> ScanFolderAsync(ScanFolderDto dto, CancellationToken ct = default);
    bool IsExtensionAllowed(string extension);
    bool IsLockedDocumentType(string documentType);
    string[] GetLockedDocumentTypes();
    Task DeleteByProjectAsync(int projectId, CancellationToken ct = default);
}
