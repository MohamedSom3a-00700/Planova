namespace Planova.Application.Dto;

public record CreateContractDto(
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
    int ClientId
);
