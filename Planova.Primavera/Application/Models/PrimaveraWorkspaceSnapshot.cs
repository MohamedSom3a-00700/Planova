using Planova.Primavera.Application.Dto;

namespace Planova.Primavera.Application.Models;

public class PrimaveraWorkspaceSnapshot
{
    public DateTime CapturedAt { get; set; }
    public List<PrimaveraActivityDto> Activities { get; set; } = new();
    public List<PrimaveraRelationshipDto> Relationships { get; set; } = new();
    public List<PrimaveraResourceAssignmentDto> ResourceAssignments { get; set; } = new();
    public List<PrimaveraCalendarDto> Calendars { get; set; } = new();
    public List<PrimaveraCodeDto> Codes { get; set; } = new();
    public List<PrimaveraBaselineDto> Baselines { get; set; } = new();
    public List<PrimaveraUdfDto> Udfs { get; set; } = new();
}
