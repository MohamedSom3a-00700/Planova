using System.IO;
using Planova.Application.Dto;
using Planova.Application.Interfaces;
using Planova.Application.Services;

namespace Planova.UI.Services;

public interface IBoqFileUploadService
{
    Task<FileUploadResult> UploadToProjectAsync(int projectId, string sourceFilePath, string? documentsFolder, CancellationToken ct);
    Task<DuplicateCheckResult> CheckDuplicateAsync(int projectId, string fileName, string sourceFilePath, CancellationToken ct);
}

public sealed record FileUploadResult(
    bool Success,
    string? DestinationPath,
    string? ErrorMessage,
    bool WasDuplicate
);

public sealed record DuplicateCheckResult(
    bool IsDuplicate,
    ProjectDocumentDto? ExistingDocument,
    string? Message
);

public sealed class BoqFileUploadService : IBoqFileUploadService
{
    private readonly IProjectDocumentService _projectDocumentService;
    private readonly IProjectFolderService _projectFolderService;

    public BoqFileUploadService(
        IProjectDocumentService projectDocumentService,
        IProjectFolderService projectFolderService)
    {
        _projectDocumentService = projectDocumentService;
        _projectFolderService = projectFolderService;
    }

    public async Task<DuplicateCheckResult> CheckDuplicateAsync(int projectId, string fileName, string sourceFilePath, CancellationToken ct)
    {
        var existingDocs = await _projectDocumentService.GetByTypeAsync(projectId, "Boq", ct);
        var fileNameOnly = Path.GetFileName(fileName);

        var duplicate = existingDocs.FirstOrDefault(d =>
            d.FileName.Equals(fileNameOnly, StringComparison.OrdinalIgnoreCase));

        if (duplicate is not null)
        {
            return new DuplicateCheckResult(true, duplicate,
                $"A BOQ document named '{fileNameOnly}' already exists in this project (uploaded {duplicate.UploadedAt:MMM dd, yyyy}). The existing document will be used for import.");
        }

        var allDocs = await _projectDocumentService.GetByProjectAsync(projectId, ct);
        var sameSize = allDocs.FirstOrDefault(d =>
            d.FileName.Equals(fileNameOnly, StringComparison.OrdinalIgnoreCase) &&
            d.FileSizeBytes == new FileInfo(sourceFilePath).Length);

        if (sameSize is not null)
        {
            return new DuplicateCheckResult(true, sameSize,
                $"A document named '{fileNameOnly}' with identical size already exists as '{sameSize.DocumentType}'.");
        }

        return new DuplicateCheckResult(false, null, null);
    }

    public async Task<FileUploadResult> UploadToProjectAsync(int projectId, string sourceFilePath, string? documentsFolder, CancellationToken ct)
    {
        if (!File.Exists(sourceFilePath))
            return new FileUploadResult(false, null, "Source file not found", false);

        var fileName = Path.GetFileName(sourceFilePath);
        var duplicate = await CheckDuplicateAsync(projectId, fileName, sourceFilePath, ct);

        if (duplicate.IsDuplicate)
            return new FileUploadResult(true, duplicate.ExistingDocument?.AbsolutePath, null, true);

        string boqFolder;
        if (!string.IsNullOrEmpty(documentsFolder))
        {
            boqFolder = Path.Combine(documentsFolder, "BOQ");
            if (!Directory.Exists(boqFolder))
            {
                try { await _projectFolderService.CreateFolderStructureAsync(documentsFolder, ct); } catch { }
                if (!Directory.Exists(boqFolder))
                    Directory.CreateDirectory(boqFolder);
            }
        }
        else
        {
            boqFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Planova", "BOQ");
            if (!Directory.Exists(boqFolder))
                Directory.CreateDirectory(boqFolder);
        }

        var destinationPath = Path.Combine(boqFolder, fileName);

        if (File.Exists(destinationPath))
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var ext = Path.GetExtension(fileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            destinationPath = Path.Combine(boqFolder, $"{nameWithoutExt}_{timestamp}{ext}");
        }

        try
        {
            File.Copy(sourceFilePath, destinationPath, false);
        }
        catch (IOException)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var ext = Path.GetExtension(fileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            destinationPath = Path.Combine(boqFolder, $"{nameWithoutExt}_{timestamp}{ext}");
            File.Copy(sourceFilePath, destinationPath, false);
        }

        try
        {
            var addDto = new AddProjectDocumentDto(projectId, destinationPath, "Boq", "Auto-uploaded during BOQ import");
            await _projectDocumentService.AddAsync(addDto, ct);
            return new FileUploadResult(true, destinationPath, null, false);
        }
        catch (Exception ex)
        {
            return new FileUploadResult(true, destinationPath, $"Document registration failed: {ex.Message}", false);
        }
    }
}
