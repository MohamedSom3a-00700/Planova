namespace Planova.Application.Dto;

public record CreateProjectDto(
    string Code,
    string Name,
    string? Description,
    string? StartDate,
    string? FinishDate,
    string? Currency,
    string? Location,
    int? ClientId,
    string? Notes
);
