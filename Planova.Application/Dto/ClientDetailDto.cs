namespace Planova.Application.Dto;

public record ClientDetailDto(
    int Id,
    string Code,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? OrganizationDetails,
    string? Logo,
    string? Notes,
    List<ProjectSummaryDto> Projects,
    List<ContractSummaryDto> Contracts,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
