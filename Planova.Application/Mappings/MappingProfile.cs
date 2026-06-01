using Planova.Application.Dto;
using Planova.Domain.Entities;
using Planova.Domain.ValueObjects;

namespace Planova.Application.Mappings;

public static class MappingProfile
{
    public static ProjectSummaryDto ToSummaryDto(this Project project)
    {
        return new ProjectSummaryDto(
            project.Id,
            project.Code,
            project.Name,
            project.Status,
            project.Client?.Name,
            project.StartDate,
            project.FinishDate,
            project.UpdatedAt
        );
    }

    public static ProjectDetailDto ToDetailDto(this Project project)
    {
        var status = ProjectStatus.TryFromString(project.Status);
        return new ProjectDetailDto(
            project.Id,
            project.Code,
            project.Name,
            project.Description,
            project.Status,
            project.StartDate,
            project.FinishDate,
            project.Currency,
            project.Location,
            project.Notes,
            project.ClientId,
            project.Client?.Name,
            project.Contracts?.Select(c => c.ToSummaryDto()).ToList() ?? new(),
            project.CreatedAt,
            project.UpdatedAt,
            status?.AllowedNext().Select(s => s.Value).ToArray() ?? Array.Empty<string>()
        );
    }

    public static ClientSummaryDto ToSummaryDto(this Client client)
    {
        return new ClientSummaryDto(
            client.Id,
            client.Code,
            client.Name,
            client.ContactEmail,
            client.Projects?.Count ?? 0,
            client.UpdatedAt
        );
    }

    public static ClientDetailDto ToDetailDto(this Client client)
    {
        return new ClientDetailDto(
            client.Id,
            client.Code,
            client.Name,
            client.ContactEmail,
            client.ContactPhone,
            client.OrganizationDetails,
            client.Notes,
            client.Projects?.Select(p => p.ToSummaryDto()).ToList() ?? new(),
            client.Contracts?.Select(c => c.ToSummaryDto()).ToList() ?? new(),
            client.CreatedAt,
            client.UpdatedAt
        );
    }

    public static ContractSummaryDto ToSummaryDto(this Contract contract)
    {
        return new ContractSummaryDto(
            contract.Id,
            contract.Number,
            contract.Title,
            contract.Value,
            contract.Currency,
            contract.Status,
            contract.Project?.Name,
            contract.Client?.Name,
            contract.UpdatedAt
        );
    }

    public static ContractDetailDto ToDetailDto(this Contract contract)
    {
        return new ContractDetailDto(
            contract.Id,
            contract.Number,
            contract.Title,
            contract.Value,
            contract.Currency,
            contract.AwardDate,
            contract.CommencementDate,
            contract.CompletionDate,
            contract.Status,
            contract.Notes,
            contract.ProjectId,
            contract.Project?.Name ?? string.Empty,
            contract.ClientId,
            contract.Client?.Name ?? string.Empty,
            contract.CreatedAt,
            contract.UpdatedAt
        );
    }
}
