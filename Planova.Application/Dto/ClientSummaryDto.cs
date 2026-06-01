namespace Planova.Application.Dto;

public record ClientSummaryDto(
    int Id,
    string Code,
    string Name,
    string? ContactEmail,
    int ProjectCount,
    DateTime UpdatedAt
);
