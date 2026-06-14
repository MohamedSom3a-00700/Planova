using System.Globalization;
using System.Text;
using Planova.Primavera.Domain.Constants;
using Planova.Primavera.Domain.Entities;

namespace Planova.Primavera.Application.Writers;

public class XerWriter
{
    public async Task WriteAsync(string filePath, List<PrimaveraCalendar> calendars,
        PrimaveraProject? project, List<PrimaveraActivity> activities,
        List<PrimaveraRelationship> relationships,
        List<PrimaveraResourceAssignment> resourceAssignments,
        List<PrimaveraCode> codes, List<PrimaveraBaseline> baselines,
        List<PrimaveraUdf> udfs, List<XerRawTable> rawTables,
        CancellationToken ct = default)
    {
        var totalRows = calendars.Count
            + (project != null ? 1 : 0)
            + activities.Count
            + relationships.Count
            + resourceAssignments.Count
            + codes.Count
            + baselines.Count
            + udfs.Count
            + rawTables.Count;
        var estimatedCapacity = Math.Max(1024, totalRows * 256);
        var sb = new StringBuilder(estimatedCapacity);
        sb.AppendLine("ERMHDR|7.0|exported|UTF-8|3.0|6.0|6.0|6.0");

        WriteCalendars(sb, calendars);
        WriteProject(sb, project);
        WriteActivities(sb, activities);
        WriteRelationships(sb, relationships);
        WriteResourceAssignments(sb, resourceAssignments);
        WriteCodes(sb, codes);
        WriteUdfs(sb, udfs);
        WriteBaselines(sb, baselines);
        WriteRawTables(sb, rawTables);

        await File.WriteAllTextAsync(filePath, sb.ToString(), ct);
    }

    private static void WriteCalendars(StringBuilder sb, List<PrimaveraCalendar> calendars)
    {
        if (calendars.Count == 0) return;
        sb.AppendLine("%T|CALENDAR");
        sb.AppendLine("%F|calendar_id|clndr_type|day_hr_cnt|week_hr_cnt|month_hr_cnt|year_hr_cnt|default_flag|name|base_clndr_id");
        foreach (var cal in calendars)
        {
            sb.Append($"%R|{cal.CalendarId}|{(cal.IsBaseCalendar ? "BASE" : "DERIVED")}|8.0|40.0|176.0|2112.0|");
            sb.Append(cal.IsBaseCalendar ? "Y" : "N");
            sb.Append($"|{Escape(cal.Name)}|{cal.BaseCalendarId ?? string.Empty}");
            sb.AppendLine();
        }
    }

    private static void WriteProject(StringBuilder sb, PrimaveraProject? project)
    {
        if (project == null) return;
        sb.AppendLine("%T|PROJECT");
        sb.AppendLine("%F|proj_id|proj_short_name|proj_name|plan_start_date|plan_end_date|add_date|status_code");
        sb.AppendLine($"%R|{project.ProjectId}|{Escape(project.Name)[..Math.Min(10, project.Name.Length)]}|{Escape(project.Name)}||||Planned");
    }

    private static void WriteActivities(StringBuilder sb, List<PrimaveraActivity> activities)
    {
        if (activities.Count == 0) return;
        sb.AppendLine("%T|TASK");
        sb.AppendLine("%F|task_id|proj_id|clndr_id|task_name|task_type|status_code|duration|start_date|end_date|phys_complete|task_code");
        foreach (var act in activities)
        {
            sb.Append($"%R|{act.TaskId}|1|{act.CalendarId ?? "1"}|{Escape(act.Name)}|TT_Task|{act.Status}|{act.Duration}");
            sb.Append($"|{FormatDate(act.StartDate)}|{FormatDate(act.EndDate)}|{act.PercentComplete}|{act.TaskId}");
            sb.AppendLine();
        }
    }

    private static void WriteRelationships(StringBuilder sb, List<PrimaveraRelationship> relationships)
    {
        if (relationships.Count == 0) return;
        sb.AppendLine("%T|TASKPRED");
        sb.AppendLine("%F|task_pred_id|task_id|pred_task_id|pred_type|lag_hr_cnt");
        int id = 1;
        foreach (var rel in relationships)
        {
            sb.AppendLine($"%R|{id}|{rel.SuccTaskId}|{rel.PredTaskId}|{rel.Type}|{rel.LagDuration}");
            id++;
        }
    }

    private static void WriteResourceAssignments(StringBuilder sb, List<PrimaveraResourceAssignment> assignments)
    {
        if (assignments.Count == 0) return;
        sb.AppendLine("%T|TASKRSRC");
        sb.AppendLine("%F|rsrc_id|task_id|proj_id|target_qty|remain_qty|cost_per_qty|target_cost|act_cost");
        foreach (var ra in assignments)
        {
            var targetCost = ra.Units * (double)ra.CostPerUnit;
            sb.AppendLine($"%R|{ra.ResourceId}|{ra.TaskId}|1|{ra.Units}|{ra.PlannedUnits}|{ra.CostPerUnit}|{targetCost}|{(ra.ActualUnits * (double)ra.CostPerUnit)}");
        }
    }

    private static void WriteCodes(StringBuilder sb, List<PrimaveraCode> codes)
    {
        if (codes.Count == 0) return;
        sb.AppendLine("%T|PROJECTCODE");
        sb.AppendLine("%F|proj_code_type_id|proj_id|code_value|proj_code_type|proj_code_name");
        foreach (var code in codes)
        {
            sb.AppendLine($"%R|{code.CodeTypeId}|1|{code.CodeValue}|{code.CodeType}|{Escape(code.CodeName)}");
        }
    }

    private static void WriteUdfs(StringBuilder sb, List<PrimaveraUdf> udfs)
    {
        if (udfs.Count == 0) return;
        sb.AppendLine("%T|UDFTYPE");
        sb.AppendLine("%F|udf_type_id|table_name|udf_type_name|udf_type_label|logical_data_type");
        foreach (var udf in udfs)
        {
            sb.AppendLine($"%R|{udf.UdfTypeId}|{udf.TableName}|{Escape(udf.FieldName)}|{Escape(udf.FieldName)}|{udf.FieldType}");
        }
    }

    private static void WriteBaselines(StringBuilder sb, List<PrimaveraBaseline> baselines)
    {
        if (baselines.Count == 0) return;
        sb.AppendLine("%T|PROJECTBASELINE");
        sb.AppendLine("%F|baseline_id|proj_id|baseline_name|version_number|is_active");
        foreach (var bl in baselines)
        {
            sb.AppendLine($"%R|{bl.BaselineId}|1|{Escape(bl.Name)}|{bl.VersionNumber}|{(bl.IsActive ? "Y" : "N")}");
        }
    }

    private static void WriteRawTables(StringBuilder sb, List<XerRawTable> rawTables)
    {
        foreach (var table in rawTables.GroupBy(r => r.TableName))
        {
            sb.AppendLine($"%T|{table.Key}");
            var first = table.First();
            if (!string.IsNullOrEmpty(first.ColumnHeaders))
            {
                var headers = System.Text.Json.JsonSerializer.Deserialize<List<string>>(first.ColumnHeaders);
                if (headers != null)
                    sb.AppendLine($"%F|{string.Join("|", headers)}");
            }
            foreach (var row in table)
            {
                if (!string.IsNullOrEmpty(row.Rows))
                {
                    var rows = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(row.Rows);
                    if (rows != null)
                    {
                        foreach (var r in rows)
                            sb.AppendLine($"%R|{string.Join("|", r.Values.Select(Escape))}");
                    }
                }
            }
        }
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Contains('|') ? value.Replace("|", "/") : value;
    }

    private static string FormatDate(DateTime? date)
    {
        return date?.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
