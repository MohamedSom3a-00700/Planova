namespace Planova.Application.Dto;

public record AddProjectDocumentDto(
    int ProjectId,
    string SourceFilePath,
    string DocumentType,
    string? Notes
);
