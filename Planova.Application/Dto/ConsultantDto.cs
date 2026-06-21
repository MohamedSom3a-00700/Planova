namespace Planova.Application.Dto;

public record ConsultantSummaryDto(
    int Id,
    string Code,
    string Name,
    string? ContactEmail,
    int ProjectCount,
    DateTime UpdatedAt
);

public record ConsultantDetailDto(
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

public record CreateConsultantDto(
    string Code,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? OrganizationDetails,
    string? Logo,
    string? Notes
);

public record UpdateConsultantDto(
    string Code,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? OrganizationDetails,
    string? Logo,
    string? Notes
);
