using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;

namespace Planova.Reporting.Application.Mappings;

public static class ReportingMappingProfile
{
    public static ReportTemplateDto ToDto(this ReportTemplate entity)
    {
        return new ReportTemplateDto(
            entity.Id,
            entity.ProjectId,
            entity.ReportType.ToString(),
            entity.Name,
            entity.Description,
            entity.IsDefault,
            entity.LayoutJson,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public static ReportInstanceDto ToDto(this ReportInstance entity)
    {
        return new ReportInstanceDto(
            entity.Id,
            entity.ProjectId,
            entity.ReportType.ToString(),
            entity.TemplateId,
            entity.Title,
            entity.Status.ToString(),
            entity.PeriodStart,
            entity.PeriodEnd,
            entity.GeneratedAt,
            entity.GeneratedBy,
            entity.DataSnapshotJson,
            entity.AiNarrative,
            entity.Notes,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public static ReportScheduleDto ToDto(this ReportSchedule entity)
    {
        return new ReportScheduleDto(
            entity.Id,
            entity.ProjectId,
            entity.ReportType.ToString(),
            entity.TemplateId,
            entity.Frequency.ToString(),
            entity.DayOfWeek,
            entity.DayOfMonth,
            entity.TimeOfDay.ToString(),
            entity.TimeZoneId,
            entity.ExportFormats,
            entity.IsActive,
            entity.LastRunAt,
            entity.LastStatus,
            entity.LastErrorMessage,
            entity.LastSuccessfulRunAt,
            entity.RetryCount,
            entity.MaxRetries,
            entity.NextRunAt,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public static ReportSectionDto ToDto(this ReportSection entity)
    {
        return new ReportSectionDto(
            entity.Id,
            entity.ReportInstanceId,
            entity.SectionType.ToString(),
            entity.Title,
            entity.OrderIndex,
            entity.ContentJson,
            entity.CreatedAt
        );
    }

    public static ReportExportDto ToDto(this ReportExport entity)
    {
        return new ReportExportDto(
            entity.Id,
            entity.ReportInstanceId,
            entity.Format.ToString(),
            entity.FilePath,
            entity.FileSizeBytes,
            entity.ExportedAt,
            entity.ExportedBy
        );
    }

    public static ProjectPartyDto ToDto(this ProjectParty entity)
    {
        return new ProjectPartyDto(
            entity.Id,
            entity.ProjectId,
            entity.Role.ToString(),
            entity.Name,
            entity.LogoPath,
            entity.Address,
            entity.ContactPerson,
            entity.ContactEmail,
            entity.ContactPhone,
            entity.DisplayOrder,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public static ReportSettingsDto ToDto(this ReportSettings entity)
    {
        return new ReportSettingsDto(
            entity.Id,
            entity.ProjectId,
            entity.ReportType.ToString(),
            entity.EnabledSectionsJson,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
