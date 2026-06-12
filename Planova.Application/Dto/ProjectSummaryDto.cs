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
    string? LogoPath = null
);
