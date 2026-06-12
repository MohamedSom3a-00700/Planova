using Planova.Application.Dto;
using Planova.Application.Exceptions;
using Planova.Application.Mappings;
using Planova.Application.Repositories;
using Planova.Application.Services;
using Planova.Domain.Entities;

namespace Planova.Persistence.Services;

public class ProjectDocumentService : IProjectDocumentService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".xlsx", ".xls", ".xlsm", ".dwg", ".dxf", ".doc", ".docx"
    };

    private static readonly Dictionary<string, string[]> TypeKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Boq"] = new[] { "boq", "bill of quantities", "quantity" },
        ["Spec"] = new[] { "spec", "specification" },
        ["Contract"] = new[] { "contract", "agreement" },
        ["Drawing"] = new[] { "draw", "dwg", "drawing", "dwg" }
    };

    private readonly IProjectDocumentRepository _repository;
    private readonly IProjectRepository _projectRepository;

    public ProjectDocumentService(
        IProjectDocumentRepository repository,
        IProjectRepository projectRepository)
    {
        _repository = repository;
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<ProjectDocumentDto>> GetByProjectAsync(int projectId, CancellationToken ct = default)
    {
        var documents = await _repository.GetByProjectIdAsync(projectId, ct);
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        var docsFolder = project?.DocumentsFolder;
        return documents.Select(d => d.ToDto(docsFolder));
    }

    public async Task<IEnumerable<ProjectDocumentDto>> GetByTypeAsync(int projectId, string documentType, CancellationToken ct = default)
    {
        var documents = await _repository.GetByProjectIdAndTypeAsync(projectId, documentType, ct);
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        var docsFolder = project?.DocumentsFolder;
        return documents.Select(d => d.ToDto(docsFolder));
    }

    public async Task<ProjectDocumentDto> AddAsync(AddProjectDocumentDto dto, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(dto.SourceFilePath);
        if (!IsExtensionAllowed(extension))
            throw new ValidationException($"File extension '{extension}' is not allowed.");

        var projectFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Planova", "Projects", dto.ProjectId.ToString());

        var docsFolder = Path.Combine(projectFolder, "Documents", dto.DocumentType);
        Directory.CreateDirectory(docsFolder);

        var fileName = Path.GetFileName(dto.SourceFilePath);
        var destPath = Path.Combine(docsFolder, fileName);
        var counter = 1;
        while (File.Exists(destPath))
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            destPath = Path.Combine(docsFolder, $"{nameWithoutExt}_{counter}{extension}");
            counter++;
        }

        var relativePath = Path.GetRelativePath(projectFolder, destPath);
        if (await _repository.PathExistsAsync(dto.ProjectId, relativePath, ct))
            throw new ValidationException($"Document '{fileName}' already exists in this project.");

        File.Copy(dto.SourceFilePath, destPath, overwrite: false);

        var fileInfo = new FileInfo(dto.SourceFilePath);

        var document = new ProjectDocument
        {
            ProjectId = dto.ProjectId,
            FileName = Path.GetFileName(destPath),
            RelativePath = relativePath,
            DocumentType = dto.DocumentType,
            FileExtension = extension,
            FileSizeBytes = fileInfo.Length,
            Notes = dto.Notes,
            UploadedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(document, ct);
        return created.ToDto();
    }

    public async Task DeleteAsync(int documentId, CancellationToken ct = default)
    {
        var document = await _repository.GetByIdAsync(documentId, ct);
        if (document == null)
            throw new EntityNotFoundException("ProjectDocument", documentId);

        if (IsLockedDocumentType(document.DocumentType))
            throw new ValidationException(
                $"'{document.DocumentType}' documents are required by studios and cannot be deleted. " +
                $"Locked types: {string.Join(", ", GetLockedDocumentTypes())}");

        await _repository.DeleteAsync(document, ct);
    }

    public async Task<IEnumerable<ProjectDocumentDto>> ScanFolderAsync(ScanFolderDto dto, CancellationToken ct = default)
    {
        if (!Directory.Exists(dto.FolderPath))
            throw new ValidationException($"Folder '{dto.FolderPath}' does not exist.");

        var added = new List<ProjectDocumentDto>();
        var files = Directory.EnumerateFiles(dto.FolderPath, "*.*", SearchOption.AllDirectories);

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId, ct);
        if (project == null)
            throw new EntityNotFoundException("Project", dto.ProjectId);

        project.DocumentsFolder = dto.FolderPath;
        await _projectRepository.UpdateAsync(project, ct);

        foreach (var filePath in files)
        {
            var ext = Path.GetExtension(filePath);
            if (!IsExtensionAllowed(ext))
                continue;

            var relativePath = Path.GetRelativePath(dto.FolderPath, filePath);
            if (await _repository.PathExistsAsync(dto.ProjectId, relativePath, ct))
                continue;

            var documentType = DetectDocumentType(filePath);
            var fileInfo = new FileInfo(filePath);

            var document = new ProjectDocument
            {
                ProjectId = dto.ProjectId,
                FileName = Path.GetFileName(filePath),
                RelativePath = relativePath,
                DocumentType = documentType,
                FileExtension = ext,
                FileSizeBytes = fileInfo.Length,
                UploadedAt = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(document, ct);
            added.Add(created.ToDto(dto.FolderPath));
        }

        return added;
    }

    public async Task DeleteByProjectAsync(int projectId, CancellationToken ct = default)
    {
        await _repository.DeleteByProjectIdAsync(projectId, ct);
    }

    public bool IsExtensionAllowed(string extension)
    {
        return AllowedExtensions.Contains(extension);
    }

    public bool IsLockedDocumentType(string documentType)
    {
        return DocumentTypeRegistry.IsLockedType(documentType);
    }

    public string[] GetLockedDocumentTypes()
    {
        return DocumentTypeRegistry.LockedTypes.ToArray();
    }

    private static string DetectDocumentType(string filePath)
    {
        var directoryName = Path.GetFileName(Path.GetDirectoryName(filePath));
        if (!string.IsNullOrEmpty(directoryName))
        {
            foreach (var kvp in TypeKeywords)
            {
                if (kvp.Value.Any(k => directoryName.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    return kvp.Key;
            }
        }

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        foreach (var kvp in TypeKeywords)
        {
            if (kvp.Value.Any(k => fileName.Contains(k, StringComparison.OrdinalIgnoreCase)))
                return kvp.Key;
        }

        return "Other";
    }
}
