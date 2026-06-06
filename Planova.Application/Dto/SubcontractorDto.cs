namespace Planova.Application.Dto;

public record SubcontractorSummaryDto(
    int Id,
    string Code,
    string Name,
    string? ContactEmail,
    string? Trade,
    int ProjectCount,
    DateTime UpdatedAt
);

public record SubcontractorDetailDto(
    int Id,
    string Code,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? OrganizationDetails,
    string? Trade,
    string? LicenseNumber,
    string? Logo,
    string? Notes,
    List<ProjectSummaryDto> Projects,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateSubcontractorDto(
    string Code,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? OrganizationDetails,
    string? Trade,
    string? LicenseNumber,
    string? Logo,
    string? Notes
);

public record UpdateSubcontractorDto(
    string Code,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? OrganizationDetails,
    string? Trade,
    string? LicenseNumber,
    string? Logo,
    string? Notes
);