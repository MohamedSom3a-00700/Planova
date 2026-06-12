namespace Planova.Reporting.Application.Dto;

public record SavePartyRequest(
    Guid? Id,
    string Role,
    string Name,
    string? Address,
    string? ContactPerson,
    string? ContactEmail,
    string? ContactPhone,
    int DisplayOrder
);
