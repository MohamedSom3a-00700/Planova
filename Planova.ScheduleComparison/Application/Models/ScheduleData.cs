namespace Planova.ScheduleComparison.Application.Models;

public class ScheduleData
{
    public List<ScheduleActivity> Activities { get; set; } = new();
    public List<ScheduleRelationship> Relationships { get; set; } = new();
    public List<ScheduleResourceAssignment> ResourceAssignments { get; set; } = new();
    public List<ScheduleCalendar> Calendars { get; set; } = new();
}
