namespace Planova.Boq.Application.Dto;

public record BoqImportRequest(
    Guid ProjectId,
    string FilePath,
    Guid? MappingProfileId,
    string? SourceFormat,
    CsvImportOptions? CsvOptions
);
