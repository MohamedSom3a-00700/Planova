namespace Planova.Reporting.Application.Dto;

public record ProjectPartyDto(
    Guid Id,
    int ProjectId,
    string Role,
    string Name,
    string? LogoPath,
    string? Address,
    string? ContactPerson,
    string? ContactEmail,
    string? ContactPhone,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
