namespace Planova.Primavera.Application.Models;

public class XerStoredData
{
    public List<XerStoredActivity> Activities { get; set; } = new();
    public List<XerStoredRelationship> Relationships { get; set; } = new();
    public List<XerStoredResourceAssignment> ResourceAssignments { get; set; } = new();
    public List<XerStoredCalendar> Calendars { get; set; } = new();
    public List<XerStoredCode> Codes { get; set; } = new();
    public List<XerStoredBaseline> Baselines { get; set; } = new();
    public List<XerStoredUdf> Udfs { get; set; } = new();
    public List<XerStoredRawTable> RawTables { get; set; } = new();
    public string? RawXerContent { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime? LastRecalcDate { get; set; }
    public DateTime? PlanStartDate { get; set; }
    public DateTime? PlanEndDate { get; set; }
    public DateTime? SchedEndDate { get; set; }
    public DateTime? AddDate { get; set; }
    public DateTime? LastTasksumDate { get; set; }
    public DateTime? LastScheduleDate { get; set; }
}

public class XerStoredCalendar
{
    public string CalendarId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsBaseCalendar { get; set; }
    public string? BaseCalendarId { get; set; }
}

public class XerStoredCode
{
    public string CodeTypeId { get; set; } = string.Empty;
    public string CodeType { get; set; } = string.Empty;
    public string CodeValue { get; set; } = string.Empty;
    public string CodeName { get; set; } = string.Empty;
}

public class XerStoredBaseline
{
    public string BaselineId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public bool IsActive { get; set; }
}

public class XerStoredUdf
{
    public string UdfTypeId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
}

public class XerStoredRawTable
{
    public string TableName { get; set; } = string.Empty;
    public List<string> ColumnHeaders { get; set; } = new();
    public List<Dictionary<string, string>> Rows { get; set; } = new();
}

public class XerStoredActivity
{
    public string TaskId { get; set; } = string.Empty;
    public string? TaskCode { get; set; }
    public string? WbsId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double Duration { get; set; }
    public double OriginalDuration { get; set; }
    public double RemainingDuration { get; set; }
    public double PercentComplete { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public DateTime? EarlyStartDate { get; set; }
    public DateTime? EarlyEndDate { get; set; }
    public DateTime? LateStartDate { get; set; }
    public DateTime? LateEndDate { get; set; }
    public double TotalFloat { get; set; }
    public double FreeFloat { get; set; }
    public string? CalendarId { get; set; }
}

public class XerStoredRelationship
{
    public string PredTaskId { get; set; } = string.Empty;
    public string SuccTaskId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double LagDuration { get; set; }
}

public class XerStoredResourceAssignment
{
    public string TaskId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public double Units { get; set; }
    public decimal CostPerUnit { get; set; }
}
