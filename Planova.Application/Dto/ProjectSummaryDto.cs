namespace Planova.Application.Dto;

public record ProjectSummaryDto(
    int Id,
    string Code,
    string Name,
    string Status,
    string? ClientName,
    DateTime? StartDate,
    DateTime? FinishDate,
    DateTime UpdatedAt,
    string? ContractorName = null,
    int DocumentCount = 0,
    string? LogoPath = null,
    double? ProgressPercentage = null,
    string? HealthStatus = "Green",
    DateTime? DataDate = null,
    decimal? Budget = null,
    int ActivitiesCount = 0,
    string? CoverImagePath = null,
    string? ConnectedXerPath = null,
    string? ConnectedDatabaseName = null,
    DateTime? LastImportDate = null,
    DateTime? LastExportDate = null,
    string? ConsultantName = null
);
