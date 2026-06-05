namespace Planova.Application.Dto;

public record UpdateClientDto(
    string Code,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? OrganizationDetails,
    string? Notes
);
