using System.IO;
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
            project.UpdatedAt,
            project.Contractor?.Name,
            project.Documents?.Count ?? 0,
            project.LogoPath
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
            project.ContractorId,
            project.Contractor?.Name,
            project.SubcontractorId,
            project.Subcontractor?.Name,
            project.Contracts?.Select(c => c.ToSummaryDto()).ToList() ?? new(),
            project.CreatedAt,
            project.UpdatedAt,
            status?.AllowedNext().Select(s => s.Value).ToArray() ?? Array.Empty<string>(),
            project.LogoPath,
            project.DocumentsFolder,
            project.Latitude,
            project.Longitude,
            project.QrCodePath,
            project.Documents?.Select(d => d.ToDto()).ToList() ?? new()
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
            client.Logo,
            client.Notes,
            client.Projects?.Select(p => p.ToSummaryDto()).ToList() ?? new(),
            client.Contracts?.Select(c => c.ToSummaryDto()).ToList() ?? new(),
            client.CreatedAt,
            client.UpdatedAt
        );
    }

    public static ContractorSummaryDto ToSummaryDto(this Contractor contractor)
    {
        return new ContractorSummaryDto(
            contractor.Id,
            contractor.Code,
            contractor.Name,
            contractor.ContactEmail,
            contractor.Projects?.Count ?? 0,
            contractor.UpdatedAt
        );
    }

    public static ContractorDetailDto ToDetailDto(this Contractor contractor)
    {
        return new ContractorDetailDto(
            contractor.Id,
            contractor.Code,
            contractor.Name,
            contractor.ContactEmail,
            contractor.ContactPhone,
            contractor.OrganizationDetails,
            contractor.Logo,
            contractor.Notes,
            contractor.Projects?.Select(p => p.ToSummaryDto()).ToList() ?? new(),
            contractor.CreatedAt,
            contractor.UpdatedAt
        );
    }

    public static SubcontractorSummaryDto ToSummaryDto(this Subcontractor subcontractor)
    {
        return new SubcontractorSummaryDto(
            subcontractor.Id,
            subcontractor.Code,
            subcontractor.Name,
            subcontractor.ContactEmail,
            subcontractor.Trade,
            subcontractor.Projects?.Count ?? 0,
            subcontractor.UpdatedAt
        );
    }

    public static SubcontractorDetailDto ToDetailDto(this Subcontractor subcontractor)
    {
        return new SubcontractorDetailDto(
            subcontractor.Id,
            subcontractor.Code,
            subcontractor.Name,
            subcontractor.ContactEmail,
            subcontractor.ContactPhone,
            subcontractor.OrganizationDetails,
            subcontractor.Trade,
            subcontractor.LicenseNumber,
            subcontractor.Logo,
            subcontractor.Notes,
            subcontractor.Projects?.Select(p => p.ToSummaryDto()).ToList() ?? new(),
            subcontractor.CreatedAt,
            subcontractor.UpdatedAt
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

    public static ProjectDocumentDto ToDto(this ProjectDocument document, string? documentsFolder = null)
    {
        var absolutePath = string.IsNullOrEmpty(documentsFolder)
            ? Path.GetFullPath(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Planova", "Projects", document.ProjectId.ToString(), document.RelativePath))
            : Path.GetFullPath(Path.Combine(documentsFolder, document.RelativePath));

        return new ProjectDocumentDto(
            document.Id,
            document.ProjectId,
            document.FileName,
            document.RelativePath,
            document.DocumentType,
            document.FileExtension,
            document.FileSizeBytes,
            document.Notes,
            document.UploadedAt,
            absolutePath
        );
    }
}
