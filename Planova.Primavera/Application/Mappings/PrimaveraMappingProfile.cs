using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Entities;

namespace Planova.Primavera.Application.Mappings;

public static class PrimaveraMappingProfile
{
    public static PrimaveraProjectDto ToDto(this PrimaveraProject entity)
    {
        return new PrimaveraProjectDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            Name = entity.Name,
            SourceFileName = entity.SourceFileName,
            ImportedAt = entity.ImportedAt,
            IsActive = entity.IsActive
        };
    }

    public static PrimaveraActivityDto ToDto(this PrimaveraActivity entity)
    {
        return new PrimaveraActivityDto
        {
            Id = entity.Id,
            TaskId = entity.TaskId,
            WbsId = entity.WbsId,
            Name = entity.Name,
            Status = entity.Status,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Duration = entity.Duration,
            RemainingDuration = entity.RemainingDuration,
            PercentComplete = entity.PercentComplete,
            CalendarId = entity.CalendarId,
            SourceType = entity.SourceType.ToString(),
            UdfValues = entity.UdfValues
        };
    }

    public static PrimaveraRelationshipDto ToDto(this PrimaveraRelationship entity)
    {
        return new PrimaveraRelationshipDto
        {
            Id = entity.Id,
            PredTaskId = entity.PredTaskId,
            SuccTaskId = entity.SuccTaskId,
            Type = entity.Type,
            LagDuration = entity.LagDuration,
            SourceType = entity.SourceType.ToString()
        };
    }

    public static PrimaveraResourceAssignmentDto ToDto(this PrimaveraResourceAssignment entity)
    {
        return new PrimaveraResourceAssignmentDto
        {
            Id = entity.Id,
            TaskId = entity.TaskId,
            ResourceId = entity.ResourceId,
            Units = entity.Units,
            PlannedUnits = entity.PlannedUnits,
            ActualUnits = entity.ActualUnits,
            CostPerUnit = entity.CostPerUnit,
            SourceType = entity.SourceType.ToString()
        };
    }

    public static PrimaveraCalendarDto ToDto(this PrimaveraCalendar entity)
    {
        return new PrimaveraCalendarDto
        {
            Id = entity.Id,
            CalendarId = entity.CalendarId,
            Name = entity.Name,
            IsBaseCalendar = entity.IsBaseCalendar,
            BaseCalendarId = entity.BaseCalendarId,
            SourceType = entity.SourceType.ToString()
        };
    }

    public static PrimaveraCodeDto ToDto(this PrimaveraCode entity)
    {
        return new PrimaveraCodeDto
        {
            Id = entity.Id,
            CodeType = entity.CodeType,
            CodeTypeId = entity.CodeTypeId,
            CodeValue = entity.CodeValue,
            CodeName = entity.CodeName,
            ParentCodeId = entity.ParentCodeId,
            SourceType = entity.SourceType.ToString()
        };
    }

    public static PrimaveraBaselineDto ToDto(this PrimaveraBaseline entity)
    {
        return new PrimaveraBaselineDto
        {
            Id = entity.Id,
            BaselineId = entity.BaselineId,
            Name = entity.Name,
            VersionNumber = entity.VersionNumber,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            SourceType = entity.SourceType.ToString()
        };
    }

    public static PrimaveraUdfDto ToDto(this PrimaveraUdf entity)
    {
        return new PrimaveraUdfDto
        {
            Id = entity.Id,
            UdfTypeId = entity.UdfTypeId,
            TableName = entity.TableName,
            FieldName = entity.FieldName,
            FieldType = entity.FieldType,
            SourceType = entity.SourceType.ToString()
        };
    }

    public static PrimaveraValidationIssueDto ToDto(this PrimaveraValidationIssue entity)
    {
        return new PrimaveraValidationIssueDto
        {
            Id = entity.Id,
            Severity = entity.Severity.ToString(),
            EntityType = entity.EntityType.ToString(),
            Description = entity.Description,
            SuggestedFix = entity.SuggestedFix,
            IsResolved = entity.IsResolved
        };
    }

    public static PrimaveraRepairActionDto ToDto(this PrimaveraRepairAction entity)
    {
        return new PrimaveraRepairActionDto
        {
            Id = entity.Id,
            Description = entity.Description,
            TargetEntityType = entity.TargetEntityType.ToString(),
            AppliedBy = entity.AppliedBy,
            AppliedAt = entity.AppliedAt,
            UndoAvailable = entity.UndoAvailable
        };
    }
}
