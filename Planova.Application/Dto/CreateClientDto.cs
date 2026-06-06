namespace Planova.Application.Dto;

public record CreateClientDto(
    string Code,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? OrganizationDetails,
    string? Logo,
    string? Notes
);