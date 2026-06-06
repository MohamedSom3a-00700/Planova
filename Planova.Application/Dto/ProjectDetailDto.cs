namespace Planova.Application.Dto;

public record ProjectDetailDto(
    int Id,
    string Code,
    string Name,
    string? Description,
    string Status,
    DateTime? StartDate,
    DateTime? FinishDate,
    string? Currency,
    string? Location,
    string? Notes,
    int? ClientId,
    string? ClientName,
    int? ContractorId,
    string? ContractorName,
    int? SubcontractorId,
    string? SubcontractorName,
    List<ContractSummaryDto> Contracts,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string[] AllowedNextStatuses
);
