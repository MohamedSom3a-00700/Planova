namespace Planova.Application.Dto;

public record ContractSummaryDto(
    int Id,
    string Number,
    string Title,
    decimal? Value,
    string? Currency,
    string? Status,
    string? ProjectName,
    string? ClientName,
    DateTime UpdatedAt
);
