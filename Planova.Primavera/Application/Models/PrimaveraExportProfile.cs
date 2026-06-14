namespace Planova.Primavera.Application.Models;

public class PrimaveraExportProfile
{
    public int ProjectId { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public bool IncludeActivities { get; set; } = true;
    public bool IncludeRelationships { get; set; } = true;
    public bool IncludeResourceAssignments { get; set; } = true;
    public bool IncludeCalendars { get; set; } = true;
    public bool IncludeCodes { get; set; } = true;
    public bool IncludeBaselines { get; set; } = true;
    public bool IncludeUdfs { get; set; } = true;
    public bool PreserveRawTables { get; set; } = true;
}
