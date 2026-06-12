namespace Planova.Application.Dto;

public record ProjectDocumentDto(
    int Id,
    int ProjectId,
    string FileName,
    string RelativePath,
    string DocumentType,
    string FileExtension,
    long FileSizeBytes,
    string? Notes,
    DateTime UploadedAt,
    string AbsolutePath
);
