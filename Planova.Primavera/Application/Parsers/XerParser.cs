using System.Globalization;
using System.Text;
using System.Text.Json;
using Planova.Primavera.Domain.Constants;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Application.Parsers;

public class XerParserResult
{
    public PrimaveraProject? Project { get; set; }
    public List<PrimaveraActivity> Activities { get; set; } = new();
    public List<PrimaveraRelationship> Relationships { get; set; } = new();
    public List<PrimaveraResourceAssignment> ResourceAssignments { get; set; } = new();
    public List<PrimaveraCalendar> Calendars { get; set; } = new();
    public List<PrimaveraCode> Codes { get; set; } = new();
    public List<PrimaveraBaseline> Baselines { get; set; } = new();
    public List<PrimaveraUdf> Udfs { get; set; } = new();
    public List<XerRawTable> RawTables { get; set; } = new();
    public Dictionary<string, int> RowCounts { get; set; } = new();
    public HashSet<string> TableNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class XerParser
{
    private static readonly string[] DateFormats = { "dd-MMM-yyyy", "dd-MMM-yy", "yyyy-MM-dd", "MM/dd/yyyy" };

    public async Task<XerParserResult> ParseAsync(string filePath, CancellationToken ct = default)
    {
        var result = new XerParserResult();

        if (!File.Exists(filePath))
        {
            result.Errors.Add($"File not found: {filePath}");
            return result;
        }

        var sessionId = Guid.NewGuid();
        var rawTableAccumulator = new Dictionary<string, List<Dictionary<string, string>>>(StringComparer.OrdinalIgnoreCase);

        string currentTable = string.Empty;
        string[]? currentFields = null;
        bool headerFound = false;

        using var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            ct.ThrowIfCancellationRequested();

            if (line.Length == 0) continue;

            if (!headerFound)
            {
                if (!line.StartsWith(XerFieldNames.HeaderRecord, StringComparison.Ordinal))
                {
                    result.Errors.Add("Invalid XER file: missing or malformed header.");
                    return result;
                }
                headerFound = true;

                var headerParts = line.Split('|');
                if (headerParts.Length > 3)
                {
                    var declaredEncoding = headerParts[3].Trim();
                    if (!string.Equals(declaredEncoding, "UTF-8", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(declaredEncoding, "UTF8", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Warnings.Add(
                            $"XER file declares encoding '{declaredEncoding}'. " +
                            "Parsing as UTF-8; some characters may not display correctly.");
                    }
                }
                continue;
            }

            if (line[0] != '%') continue;

            if (line.Length < 3) continue;

            char marker = line[1];
            if (marker is 'T' or 't')
            {
                currentTable = line[2..].Trim();
                currentFields = null;
                if (!string.IsNullOrEmpty(currentTable))
                    result.TableNames.Add(currentTable);
            }
            else if (marker is 'F' or 'f')
            {
                currentFields = line[2..].Split('|', StringSplitOptions.TrimEntries);
            }
            else if (marker is 'R' or 'r' && currentFields != null)
            {
                var values = line[2..].Split('|');
                try
                {
                    ParseRow(result, currentTable, currentFields, values, sessionId, rawTableAccumulator);
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"Error parsing row in table '{currentTable}': {ex.Message}");
                }
            }
        }

        foreach (var (tableName, rows) in rawTableAccumulator)
        {
            if (rows.Count == 0) continue;

            var headers = rows[0].Keys.ToList();
            var rawTable = new XerRawTable
            {
                Id = Guid.NewGuid(),
                ImportSessionId = sessionId,
                TableName = tableName,
                ColumnHeaders = JsonSerializer.Serialize(headers),
                Rows = JsonSerializer.Serialize(rows),
                SortOrder = result.RawTables.Count + 1
            };
            result.RawTables.Add(rawTable);
            IncrementCount(result, "RawTable");
        }

        return result;
    }

    private void ParseRow(
        XerParserResult result,
        string table,
        string[] fields,
        string[] values,
        Guid sessionId,
        Dictionary<string, List<Dictionary<string, string>>> rawTableAccumulator)
    {
        var row = new Dictionary<string, string>(fields.Length, StringComparer.OrdinalIgnoreCase);
        int minLen = Math.Min(fields.Length, values.Length);
        for (int i = 0; i < minLen; i++)
        {
            row[fields[i]] = values[i];
        }

        switch (table.ToUpperInvariant())
        {
            case "CALENDAR":
                ParseCalendar(result, row, sessionId);
                break;
            case "PROJECT":
                ParseProject(result, row, sessionId);
                break;
            case "TASK":
                ParseTask(result, row, sessionId);
                break;
            case "TASKPRED":
                ParseRelationship(result, row, sessionId);
                break;
            case "TASKRSRC":
                ParseResourceAssignment(result, row, sessionId);
                break;
            case "RSOURCE":
                break;
            case "PROJECTCODE":
            case "PROJCODECAT":
            case "PROJCODEVAL":
                ParseCode(result, table, row, sessionId);
                break;
            case "UDFTYPE":
                ParseUdfType(result, row, sessionId);
                break;
            case "UDFVALUE":
                break;
            case "PROJECTBASELINE":
                ParseBaseline(result, row, sessionId);
                break;
            default:
                if (!XerFieldNames.IsSupportedTable(table))
                {
                    ParseRawTable(table, row, rawTableAccumulator);
                }
                break;
        }
    }

    private static void ParseRawTable(
        string table,
        Dictionary<string, string> row,
        Dictionary<string, List<Dictionary<string, string>>> accumulator)
    {
        if (!accumulator.TryGetValue(table, out var rows))
        {
            rows = new List<Dictionary<string, string>>();
            accumulator[table] = rows;
        }
        rows.Add(row);
    }

    private void ParseCalendar(XerParserResult result, Dictionary<string, string> row, Guid sessionId)
    {
        var calendar = new PrimaveraCalendar
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            CalendarId = row.GetValueOrDefault("calendar_id", string.Empty),
            Name = row.GetValueOrDefault("name", string.Empty),
            IsBaseCalendar = row.GetValueOrDefault("clndr_type", "") == "BASE",
            BaseCalendarId = row.GetValueOrDefault("base_clndr_id"),
            ImportSessionId = sessionId
        };
        result.Calendars.Add(calendar);
        IncrementCount(result, nameof(PrimaveraEntityType.Calendar));
    }

    private void ParseProject(XerParserResult result, Dictionary<string, string> row, Guid sessionId)
    {
        if (result.Project != null) return;

        result.Project = new PrimaveraProject
        {
            Id = Guid.NewGuid(),
            ProjectId = row.GetValueOrDefault("proj_id", string.Empty),
            Name = row.GetValueOrDefault("proj_name", string.Empty),
            SourceFileName = string.Empty,
            ImportedAt = DateTime.UtcNow,
            IsActive = true,
            ImportSessionId = 0
        };
        IncrementCount(result, nameof(PrimaveraEntityType.Project));
    }

    private void ParseTask(XerParserResult result, Dictionary<string, string> row, Guid sessionId)
    {
        var start = TryParseDate(row.GetValueOrDefault("start_date", ""));
        var end = TryParseDate(row.GetValueOrDefault("end_date", ""));

        var activity = new PrimaveraActivity
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            TaskId = row.GetValueOrDefault("task_id", string.Empty),
            WbsId = row.GetValueOrDefault("wbs_id"),
            Name = row.GetValueOrDefault("task_name", string.Empty),
            Status = row.GetValueOrDefault("status_code", string.Empty),
            StartDate = start,
            EndDate = end,
            Duration = ParseDouble(row.GetValueOrDefault("duration", "0")),
            RemainingDuration = ParseDouble(row.GetValueOrDefault("remain_dur", row.GetValueOrDefault("duration", "0"))),
            PercentComplete = ParseDouble(row.GetValueOrDefault("phys_complete", "0")),
            CalendarId = row.GetValueOrDefault("clndr_id"),
            SourceType = PrimaveraSourceType.Imported,
            CreatedAt = DateTime.UtcNow,
            ImportSessionId = sessionId
        };
        result.Activities.Add(activity);
        IncrementCount(result, nameof(PrimaveraEntityType.Activity));
    }

    private void ParseRelationship(XerParserResult result, Dictionary<string, string> row, Guid sessionId)
    {
        var relationship = new PrimaveraRelationship
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            PredTaskId = row.GetValueOrDefault("pred_task_id", string.Empty),
            SuccTaskId = row.GetValueOrDefault("task_id", string.Empty),
            Type = row.GetValueOrDefault("pred_type", "FS"),
            LagDuration = ParseDouble(row.GetValueOrDefault("lag_hr_cnt", "0")),
            ImportSessionId = sessionId
        };
        result.Relationships.Add(relationship);
        IncrementCount(result, nameof(PrimaveraEntityType.Relationship));
    }

    private void ParseResourceAssignment(XerParserResult result, Dictionary<string, string> row, Guid sessionId)
    {
        var assignment = new PrimaveraResourceAssignment
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            TaskId = row.GetValueOrDefault("task_id", string.Empty),
            ResourceId = row.GetValueOrDefault("rsrc_id", string.Empty),
            Units = ParseDouble(row.GetValueOrDefault("target_qty", "0")),
            PlannedUnits = ParseDouble(row.GetValueOrDefault("target_qty", "0")),
            ActualUnits = ParseDouble(row.GetValueOrDefault("act_this_per_qty", "0")),
            CostPerUnit = ParseDecimal(row.GetValueOrDefault("cost_per_qty", "0")),
            ImportSessionId = sessionId
        };
        result.ResourceAssignments.Add(assignment);
        IncrementCount(result, nameof(PrimaveraEntityType.ResourceAssignment));
    }

    private void ParseCode(XerParserResult result, string table, Dictionary<string, string> row, Guid sessionId)
    {
        var code = new PrimaveraCode
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            CodeType = table switch
            {
                "PROJECTCODE" => "ProjectCode",
                "PROJCODECAT" => "ProjectCodeCategory",
                "PROJCODEVAL" => "ProjectCodeValue",
                _ => "Unknown"
            },
            CodeTypeId = row.GetValueOrDefault("proj_code_type_id", row.GetValueOrDefault("proj_code_cat_id", "")),
            CodeValue = row.GetValueOrDefault("code_value", row.GetValueOrDefault("proj_code_value", "")),
            CodeName = row.GetValueOrDefault("proj_code_name", row.GetValueOrDefault("code_name", "")),
            ParentCodeId = row.GetValueOrDefault("parent_code_id"),
            ImportSessionId = sessionId
        };
        result.Codes.Add(code);
        IncrementCount(result, nameof(PrimaveraEntityType.Code));
    }

    private void ParseUdfType(XerParserResult result, Dictionary<string, string> row, Guid sessionId)
    {
        var udf = new PrimaveraUdf
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            UdfTypeId = row.GetValueOrDefault("udf_type_id", string.Empty),
            TableName = row.GetValueOrDefault("table_name", string.Empty),
            FieldName = row.GetValueOrDefault("udf_type_name", string.Empty),
            FieldType = row.GetValueOrDefault("logical_data_type", string.Empty),
            ImportSessionId = sessionId
        };
        result.Udfs.Add(udf);
        IncrementCount(result, nameof(PrimaveraEntityType.Udf));
    }

    private void ParseBaseline(XerParserResult result, Dictionary<string, string> row, Guid sessionId)
    {
        var baseline = new PrimaveraBaseline
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            BaselineId = row.GetValueOrDefault("baseline_id", string.Empty),
            Name = row.GetValueOrDefault("baseline_name", string.Empty),
            VersionNumber = (int)ParseDouble(row.GetValueOrDefault("version_number", "1")),
            IsActive = row.GetValueOrDefault("is_active", "N") == "Y",
            CreatedAt = DateTime.UtcNow,
            ImportSessionId = sessionId
        };
        result.Baselines.Add(baseline);
        IncrementCount(result, nameof(PrimaveraEntityType.Baseline));
    }

    private void IncrementCount(XerParserResult result, string key)
    {
        if (result.RowCounts.ContainsKey(key))
            result.RowCounts[key]++;
        else
            result.RowCounts[key] = 1;
    }

    private static DateTime? TryParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParseExact(value, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return date;
        return null;
    }

    private static double ParseDouble(string value)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        return 0;
    }

    private static decimal ParseDecimal(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        return 0;
    }
}
