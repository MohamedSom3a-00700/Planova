namespace Planova.Application.Dto;

public record ContractDetailDto(
    int Id,
    string Number,
    string Title,
    decimal? Value,
    string? Currency,
    DateTime? AwardDate,
    DateTime? CommencementDate,
    DateTime? CompletionDate,
    string? Status,
    string? Notes,
    int ProjectId,
    string ProjectName,
    int ClientId,
    string ClientName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
