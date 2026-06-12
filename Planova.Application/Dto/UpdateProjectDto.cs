namespace Planova.Application.Dto;

public record UpdateProjectDto(
    string Code,
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? FinishDate,
    string? Currency,
    string? Location,
    int? ClientId,
    int? ContractorId,
    int? SubcontractorId,
    string? Notes,
    string? LogoSourcePath = null,
    string? DocumentsFolder = null,
    double? Latitude = null,
    double? Longitude = null
);
