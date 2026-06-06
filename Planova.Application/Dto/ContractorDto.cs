namespace Planova.Application.Dto;

public record ContractorSummaryDto(
    int Id,
    string Code,
    string Name,
    string? ContactEmail,
    int ProjectCount,
    DateTime UpdatedAt
);

public record ContractorDetailDto(
    int Id,
    string Code,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? OrganizationDetails,
    string? Logo,
    string? Notes,
    List<ProjectSummaryDto> Projects,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateContractorDto(
    string Code,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? OrganizationDetails,
    string? Logo,
    string? Notes
);

public record UpdateContractorDto(
    string Code,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? OrganizationDetails,
    string? Logo,
    string? Notes
);