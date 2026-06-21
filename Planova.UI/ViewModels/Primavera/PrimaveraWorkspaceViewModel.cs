using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Application.Models;
using Planova.Primavera.Domain.Interfaces;
using ClosedXML.Excel;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Enums;

namespace Planova.UI.ViewModels.Primavera;

public partial class EntityTypeSummary
{
    public string TypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Details { get; set; } = string.Empty;
}

public partial class XeroTableSection : ObservableObject
{
    [ObservableProperty]
    private string _tableName = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}

public partial class PrimaveraWorkspaceViewModel : ObservableObject
{
    private readonly IPrimaveraWorkspaceService _workspaceService;
    private readonly IPrimaveraImportService _importService;
    private int _currentProjectId;

    public Action? OnNavigateToValidation { get; set; }

    public PrimaveraWorkspaceViewModel(
        IPrimaveraWorkspaceService workspaceService,
        IPrimaveraImportService importService,
        PrimaveraCalendarsViewModel calendarsVm,
        PrimaveraCodesViewModel codesVm,
        PrimaveraActivitiesViewModel activitiesVm,
        PrimaveraRelationshipsViewModel relationshipsVm,
        PrimaveraResourcesViewModel resourcesVm,
        PrimaveraBaselinesViewModel baselinesVm,
        PrimaveraUdfsViewModel udfsVm)
    {
        _workspaceService = workspaceService;
        _importService = importService;
        CalendarsViewModel = calendarsVm;
        CodesViewModel = codesVm;
        ActivitiesViewModel = activitiesVm;
        RelationshipsViewModel = relationshipsVm;
        ResourcesViewModel = resourcesVm;
        BaselinesViewModel = baselinesVm;
        UdfsViewModel = udfsVm;
    }

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<XerImportSessionDto> ImportedSessions { get; } = new();

    [ObservableProperty]
    private XerImportSessionDto? _selectedSession;

    public ObservableCollection<EntityTypeSummary> EntitySummaries { get; } = new();

    public ObservableCollection<XeroTableSection> XeroTableSections { get; } = new();

    partial void OnSelectedSessionChanged(XerImportSessionDto? value)
    {
        if (value != null)
        {
            StatusMessage = $"Loaded: {value.SourceFileName} ({value.ImportedAt:g})";
            UpdateEntitySummaries(value);
            UpdateTableSections(value);
            LoadTabsFromSessionData(value);
        }
    }

    private void LoadTabsFromSessionData(XerImportSessionDto session)
    {
        if (string.IsNullOrEmpty(session.ParsedDataJson))
        {
            ActivitiesViewModel.Activities.Clear();
            RelationshipsViewModel.Relationships.Clear();
            ResourcesViewModel.ResourceAssignments.Clear();
            CalendarsViewModel.Calendars.Clear();
            CodesViewModel.Codes.Clear();
            BaselinesViewModel.Baselines.Clear();
            UdfsViewModel.Udfs.Clear();
            return;
        }

        try
        {
            var data = JsonSerializer.Deserialize<XerStoredData>(session.ParsedDataJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (data == null) return;

            ActivitiesViewModel.Activities.Clear();
            foreach (var a in data.Activities)
                ActivitiesViewModel.Activities.Add(new PrimaveraActivityDto
                {
                    TaskId = a.TaskId,
                    WbsId = a.WbsId,
                    Name = a.Name,
                    Status = a.Status,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Duration = a.Duration,
                    RemainingDuration = a.RemainingDuration,
                    PercentComplete = a.PercentComplete,
                    CalendarId = a.CalendarId,
                    SourceType = PrimaveraSourceType.Imported.ToString()
                });

            RelationshipsViewModel.Relationships.Clear();
            foreach (var r in data.Relationships)
                RelationshipsViewModel.Relationships.Add(new PrimaveraRelationshipDto
                {
                    PredTaskId = r.PredTaskId,
                    SuccTaskId = r.SuccTaskId,
                    Type = r.Type,
                    LagDuration = r.LagDuration
                });

            ResourcesViewModel.ResourceAssignments.Clear();
            foreach (var ra in data.ResourceAssignments)
                ResourcesViewModel.ResourceAssignments.Add(new PrimaveraResourceAssignmentDto
                {
                    TaskId = ra.TaskId,
                    ResourceId = ra.ResourceId,
                    Units = ra.Units,
                    CostPerUnit = ra.CostPerUnit
                });

            CalendarsViewModel.Calendars.Clear();
            foreach (var c in data.Calendars)
                CalendarsViewModel.Calendars.Add(new PrimaveraCalendarDto
                {
                    CalendarId = c.CalendarId,
                    Name = c.Name,
                    IsBaseCalendar = c.IsBaseCalendar,
                    BaseCalendarId = c.BaseCalendarId
                });

            CodesViewModel.Codes.Clear();
            foreach (var c in data.Codes)
                CodesViewModel.Codes.Add(new PrimaveraCodeDto
                {
                    CodeTypeId = c.CodeTypeId,
                    CodeType = c.CodeType,
                    CodeValue = c.CodeValue,
                    CodeName = c.CodeName
                });

            BaselinesViewModel.Baselines.Clear();
            foreach (var b in data.Baselines)
                BaselinesViewModel.Baselines.Add(new PrimaveraBaselineDto
                {
                    BaselineId = b.BaselineId,
                    Name = b.Name,
                    VersionNumber = b.VersionNumber,
                    IsActive = b.IsActive
                });

            UdfsViewModel.Udfs.Clear();
            foreach (var u in data.Udfs)
                UdfsViewModel.Udfs.Add(new PrimaveraUdfDto
                {
                    UdfTypeId = u.UdfTypeId,
                    TableName = u.TableName,
                    FieldName = u.FieldName,
                    FieldType = u.FieldType
                });

        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading session data: {ex.Message}";
        }
    }

    private void UpdateEntitySummaries(XerImportSessionDto session)
    {
        EntitySummaries.Clear();
        var counts = session.RowCountsDict;
        if (counts.Count == 0)
        {
            EntitySummaries.Add(new EntityTypeSummary { TypeName = "No data", Count = 0, Details = "No row counts available." });
            return;
        }
        foreach (var kv in counts)
        {
            EntitySummaries.Add(new EntityTypeSummary
            {
                TypeName = kv.Key,
                Count = kv.Value,
                Details = $"{kv.Value} row(s)"
            });
        }
    }

    private void UpdateTableSections(XerImportSessionDto session)
    {
        XeroTableSections.Clear();
        var tableNames = session.TableNamesList;
        if (tableNames.Count == 0)
        {
            XeroTableSections.Add(new XeroTableSection { TableName = "(no tables)", IsSelected = false });
            return;
        }
        foreach (var name in tableNames)
            XeroTableSections.Add(new XeroTableSection { TableName = name, IsSelected = false });
    }

    [RelayCommand]
    private async Task ExportSelectedTablesAsync()
    {
        var selected = XeroTableSections.Where(t => t.IsSelected).ToList();
        if (selected.Count == 0)
        {
            StatusMessage = "No tables selected for export.";
            return;
        }

        var xerContent = GetRawXerContent();

        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"Planova_XERTables_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            using var workbook = new XLWorkbook();

            foreach (var section in selected)
            {
                var tableName = section.TableName;
                var ws = workbook.Worksheets.Add(tableName.Length > 31 ? tableName[..31] : tableName);

                if (!string.IsNullOrEmpty(xerContent))
                {
                    var tableContent = ExtractXerTable(xerContent, tableName);
                    if (tableContent.Count == 0)
                    {
                        ws.Cell(1, 1).Value = $"(no data for table '{tableName}')";
                    }
                    else
                    {
                        int currentRow = 1;
                        foreach (var line in tableContent)
                        {
                            var parts = line.Split('\t');
                            for (int c = 0; c < parts.Length; c++)
                                ws.Cell(currentRow, c + 1).Value = parts[c];
                            currentRow++;
                        }
                        ws.Row(1).Style.Font.Bold = true;
                        ws.Columns().AdjustToContents();
                    }
                }
                else
                {
                    ExportFromViewModel(ws, tableName);
                }
            }

            workbook.SaveAs(tempPath);
            StatusMessage = $"Exported {selected.Count} table(s) to Excel: {tempPath}";

            var process = Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            if (process != null)
            {
                await Task.Run(() => process.WaitForExit());
                await Task.Delay(500);
                await ImportFromExcelAsync(tempPath);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Table export failed: {ex.Message}";
        }
    }

    private void ExportFromViewModel(IXLWorksheet ws, string tableName)
    {
        int currentRow = 1;

        switch (tableName.ToUpperInvariant())
        {
            case "TASK":
            case "ACTIVITY":
                ExportTypedTable(ws, ActivitiesViewModel.Activities, ref currentRow);
                break;
            case "TASKPRED":
            case "RELATIONSHIP":
                ExportTypedTable(ws, RelationshipsViewModel.Relationships, ref currentRow);
                break;
            case "TASKRSRC":
            case "RESOURCE":
                ExportTypedTable(ws, ResourcesViewModel.ResourceAssignments, ref currentRow);
                break;
            case "CALENDAR":
                ExportTypedTable(ws, CalendarsViewModel.Calendars, ref currentRow);
                break;
            case "PROJECTCODE":
            case "PROJCODECAT":
            case "PROJCODEVAL":
            case "CODE":
                ExportTypedTable(ws, CodesViewModel.Codes, ref currentRow);
                break;
            case "UDFTYPE":
            case "UDF":
                ExportTypedTable(ws, UdfsViewModel.Udfs, ref currentRow);
                break;
            case "PROJECTBASELINE":
            case "BASELINE":
                ExportTypedTable(ws, BaselinesViewModel.Baselines, ref currentRow);
                break;
            default:
                ws.Cell(1, 1).Value = $"(table '{tableName}' not available from current session data)";
                break;
        }

        ws.Columns().AdjustToContents();
    }

    private string? GetRawXerContent()
    {
        if (SelectedSession == null || string.IsNullOrEmpty(SelectedSession.ParsedDataJson))
            return null;

        var data = JsonSerializer.Deserialize<XerStoredData>(SelectedSession.ParsedDataJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return data?.RawXerContent;
    }

    private static List<string> ExtractXerTable(string xerContent, string tableName)
    {
        var result = new List<string>();
        using var reader = new StringReader(xerContent);
        string? line;
        bool inTargetTable = false;

        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith("%T\t", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("%T ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(new[] { '\t', ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                var currentTable = parts.Length > 1 ? parts[1].Trim() : "";

                if (currentTable.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                {
                    inTargetTable = true;
                    continue;
                }

                if (inTargetTable)
                    break;
            }

            if (inTargetTable)
            {
                if (line.StartsWith('%'))
                    result.Add(line[3..]);
            }
        }

        return result;
    }

    private static void ExportTypedTable<T>(IXLWorksheet ws, IList<T> items, ref int currentRow)
    {
        if (items.Count == 0)
        {
            ws.Cell(currentRow, 1).Value = "(no data)";
            currentRow++;
            return;
        }

        var props = items.First()!.GetType().GetProperties();
        for (int c = 0; c < props.Length; c++)
            ws.Cell(currentRow, c + 1).Value = props[c].Name;
        ws.Row(currentRow).Style.Font.Bold = true;
        currentRow++;

        for (int r = 0; r < items.Count; r++)
        {
            for (int c = 0; c < props.Length; c++)
            {
                var val = props[c].GetValue(items[r]);
                ws.Cell(currentRow, c + 1).Value = val?.ToString() ?? "";
            }
            currentRow++;
        }
    }

    public async Task LoadSessionsAsync(CancellationToken ct = default)
    {
        try
        {
            var sessions = await _importService.GetImportedSessionsAsync(ct);
            ImportedSessions.Clear();
            foreach (var s in sessions)
                ImportedSessions.Add(s);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load sessions: {ex.Message}";
        }
    }

    public async Task LoadAsync(int projectId, CancellationToken ct = default)
    {
        _currentProjectId = projectId;
        await Task.WhenAll(
            CalendarsViewModel.LoadAsync(projectId, ct),
            CodesViewModel.LoadAsync(projectId, ct),
            ActivitiesViewModel.LoadAsync(projectId, ct),
            RelationshipsViewModel.LoadAsync(projectId, ct),
            ResourcesViewModel.LoadAsync(projectId, ct),
            BaselinesViewModel.LoadAsync(projectId, ct),
            UdfsViewModel.LoadAsync(projectId, ct));
        await LoadSessionsAsync(ct);
        StatusMessage = "Workspace data loaded.";
    }

    [RelayCommand]
    private void OpenValidation()
    {
        OnNavigateToValidation?.Invoke();
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        IsSaving = true;
        try
        {
            foreach (var item in CalendarsViewModel.Calendars)
                await _workspaceService.UpdateCalendarAsync(item);
            foreach (var item in CodesViewModel.Codes)
                await _workspaceService.UpdateCodeAsync(item);
            foreach (var item in ActivitiesViewModel.Activities)
                await _workspaceService.UpdateActivityAsync(item);
            foreach (var item in RelationshipsViewModel.Relationships)
                await _workspaceService.UpdateRelationshipAsync(item);
            foreach (var item in ResourcesViewModel.ResourceAssignments)
                await _workspaceService.UpdateResourceAssignmentAsync(item);
            foreach (var item in UdfsViewModel.Udfs)
                await _workspaceService.UpdateUdfAsync(item);

            var count = await _workspaceService.SaveChangesAsync();
            StatusMessage = $"{count} change(s) committed.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task ExportToExcelAsync()
    {
        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"Planova_Workspace_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("XER Data");

            int currentRow = 1;

            if (CalendarsViewModel.Calendars.Count > 0)
            {
                ws.Cell(currentRow, 1).Value = "=== CALENDARS ===";
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Font.FontSize = 14;
                currentRow++;

                var dt = CalendarsViewModel.Calendars.First().GetType().GetProperties();
                for (int c = 0; c < dt.Length; c++)
                    ws.Cell(currentRow, c + 1).Value = dt[c].Name;
                ws.Row(currentRow).Style.Font.Bold = true;
                currentRow++;

                for (int r = 0; r < CalendarsViewModel.Calendars.Count; r++)
                {
                    for (int c = 0; c < dt.Length; c++)
                    {
                        var val = dt[c].GetValue(CalendarsViewModel.Calendars[r]);
                        ws.Cell(currentRow, c + 1).Value = val?.ToString() ?? "";
                    }
                    currentRow++;
                }
                currentRow++;
            }

            if (CodesViewModel.Codes.Count > 0)
            {
                ws.Cell(currentRow, 1).Value = "=== CODES ===";
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Font.FontSize = 14;
                currentRow++;

                var dt = CodesViewModel.Codes.First().GetType().GetProperties();
                for (int c = 0; c < dt.Length; c++)
                    ws.Cell(currentRow, c + 1).Value = dt[c].Name;
                ws.Row(currentRow).Style.Font.Bold = true;
                currentRow++;

                for (int r = 0; r < CodesViewModel.Codes.Count; r++)
                {
                    for (int c = 0; c < dt.Length; c++)
                    {
                        var val = dt[c].GetValue(CodesViewModel.Codes[r]);
                        ws.Cell(currentRow, c + 1).Value = val?.ToString() ?? "";
                    }
                    currentRow++;
                }
                currentRow++;
            }

            if (ActivitiesViewModel.Activities.Count > 0)
            {
                ws.Cell(currentRow, 1).Value = "=== ACTIVITIES ===";
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Font.FontSize = 14;
                currentRow++;

                var dt = ActivitiesViewModel.Activities.First().GetType().GetProperties();
                for (int c = 0; c < dt.Length; c++)
                    ws.Cell(currentRow, c + 1).Value = dt[c].Name;
                ws.Row(currentRow).Style.Font.Bold = true;
                currentRow++;

                for (int r = 0; r < ActivitiesViewModel.Activities.Count; r++)
                {
                    for (int c = 0; c < dt.Length; c++)
                    {
                        var val = dt[c].GetValue(ActivitiesViewModel.Activities[r]);
                        ws.Cell(currentRow, c + 1).Value = val?.ToString() ?? "";
                    }
                    currentRow++;
                }
                currentRow++;
            }

            if (RelationshipsViewModel.Relationships.Count > 0)
            {
                ws.Cell(currentRow, 1).Value = "=== RELATIONSHIPS ===";
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Font.FontSize = 14;
                currentRow++;

                var dt = RelationshipsViewModel.Relationships.First().GetType().GetProperties();
                for (int c = 0; c < dt.Length; c++)
                    ws.Cell(currentRow, c + 1).Value = dt[c].Name;
                ws.Row(currentRow).Style.Font.Bold = true;
                currentRow++;

                for (int r = 0; r < RelationshipsViewModel.Relationships.Count; r++)
                {
                    for (int c = 0; c < dt.Length; c++)
                    {
                        var val = dt[c].GetValue(RelationshipsViewModel.Relationships[r]);
                        ws.Cell(currentRow, c + 1).Value = val?.ToString() ?? "";
                    }
                    currentRow++;
                }
                currentRow++;
            }

            if (ResourcesViewModel.ResourceAssignments.Count > 0)
            {
                ws.Cell(currentRow, 1).Value = "=== RESOURCE ASSIGNMENTS ===";
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Font.FontSize = 14;
                currentRow++;

                var dt = ResourcesViewModel.ResourceAssignments.First().GetType().GetProperties();
                for (int c = 0; c < dt.Length; c++)
                    ws.Cell(currentRow, c + 1).Value = dt[c].Name;
                ws.Row(currentRow).Style.Font.Bold = true;
                currentRow++;

                for (int r = 0; r < ResourcesViewModel.ResourceAssignments.Count; r++)
                {
                    for (int c = 0; c < dt.Length; c++)
                    {
                        var val = dt[c].GetValue(ResourcesViewModel.ResourceAssignments[r]);
                        ws.Cell(currentRow, c + 1).Value = val?.ToString() ?? "";
                    }
                    currentRow++;
                }
                currentRow++;
            }

            if (UdfsViewModel.Udfs.Count > 0)
            {
                ws.Cell(currentRow, 1).Value = "=== UDFS ===";
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Font.FontSize = 14;
                currentRow++;

                var dt = UdfsViewModel.Udfs.First().GetType().GetProperties();
                for (int c = 0; c < dt.Length; c++)
                    ws.Cell(currentRow, c + 1).Value = dt[c].Name;
                ws.Row(currentRow).Style.Font.Bold = true;
                currentRow++;

                for (int r = 0; r < UdfsViewModel.Udfs.Count; r++)
                {
                    for (int c = 0; c < dt.Length; c++)
                    {
                        var val = dt[c].GetValue(UdfsViewModel.Udfs[r]);
                        ws.Cell(currentRow, c + 1).Value = val?.ToString() ?? "";
                    }
                    currentRow++;
                }
                currentRow++;
            }

            ws.Columns().AdjustToContents();

            workbook.SaveAs(tempPath);
            StatusMessage = $"Exported to Excel: {tempPath}";

            var process = Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            if (process != null)
            {
                await Task.Run(() => process.WaitForExit());
                await Task.Delay(500);
                await ImportFromExcelAsync(tempPath);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Excel export failed: {ex.Message}";
        }
    }

    private async Task ImportFromExcelAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var ws = workbook.Worksheet(1);
            int totalChanges = 0;
            var changes = new Dictionary<string, int>();

            var allLines = new List<string[]>();
            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
            for (int r = 1; r <= lastRow; r++)
            {
                var row = ws.Row(r);
                var cells = row.Cells().Select(c => c.Value.ToString()).ToArray();
                allLines.Add(cells);
            }

            var headers = new List<string>();
            string? currentSection = null;
            var sectionHeaders = new Dictionary<string, List<string>>();
            var sectionData = new Dictionary<string, List<string[]>>();

            for (int i = 0; i < allLines.Count; i++)
            {
                var line = allLines[i];
                if (line.Length > 0 && line[0] != null && line[0].StartsWith("===") && line[0].EndsWith("==="))
                {
                    currentSection = line[0].Trim('=', ' ');
                    sectionHeaders[currentSection] = new List<string>();
                    sectionData[currentSection] = new List<string[]>();
                    continue;
                }
                if (currentSection == null) continue;

                if (sectionHeaders[currentSection].Count == 0 && line.Length > 0 && line[0] != "===")
                {
                    sectionHeaders[currentSection] = line.ToList();
                    continue;
                }

                if (sectionHeaders[currentSection].Count > 0 && line.Length > 0 && line[0] != "")
                {
                    sectionData[currentSection].Add(line);
                }
            }

            foreach (var section in sectionData)
            {
                var headerList = sectionHeaders[section.Key];
                var dataRows = section.Value;

                switch (section.Key)
                {
                    case "ACTIVITIES":
                        var actProps = ActivitiesViewModel.Activities.FirstOrDefault()?.GetType().GetProperties();
                        if (actProps == null) continue;
                        foreach (var row in dataRows)
                        {
                            if (row.Length == 0) continue;
                            var idCell = row[0];
                            var activity = ActivitiesViewModel.Activities.FirstOrDefault(a => a.Id.ToString() == idCell);
                            if (activity == null) continue;
                            for (int c = 0; c < headerList.Count && c < actProps.Length; c++)
                            {
                                var cellVal = row.Length > c ? row[c] : "";
                                var prop = actProps[c];
                                if (prop.CanWrite && prop.GetValue(activity)?.ToString() != cellVal)
                                {
                                    if (prop.PropertyType == typeof(double) && double.TryParse(cellVal, out var dVal))
                                        prop.SetValue(activity, dVal);
                                    else if (prop.PropertyType == typeof(DateTime?) && DateTime.TryParse(cellVal, out var dtVal))
                                        prop.SetValue(activity, dtVal);
                                    else if (prop.PropertyType == typeof(string))
                                        prop.SetValue(activity, cellVal);
                                    totalChanges++;
                                }
                            }
                        }
                        if (dataRows.Count > 0)
                            changes["Activities"] = dataRows.Count;
                        break;
                    case "RELATIONSHIPS":
                        var relProps = RelationshipsViewModel.Relationships.FirstOrDefault()?.GetType().GetProperties();
                        if (relProps == null) continue;
                        foreach (var row in dataRows)
                        {
                            if (row.Length == 0) continue;
                            var idCell = row[0];
                            var rel = RelationshipsViewModel.Relationships.FirstOrDefault(r => r.Id.ToString() == idCell);
                            if (rel == null) continue;
                            for (int c = 0; c < headerList.Count && c < relProps.Length; c++)
                            {
                                var cellVal = row.Length > c ? row[c] : "";
                                var prop = relProps[c];
                                if (prop.CanWrite && prop.GetValue(rel)?.ToString() != cellVal)
                                {
                                    if (prop.PropertyType == typeof(double) && double.TryParse(cellVal, out var dVal))
                                        prop.SetValue(rel, dVal);
                                    else if (prop.PropertyType == typeof(DateTime?) && DateTime.TryParse(cellVal, out var dtVal))
                                        prop.SetValue(rel, dtVal);
                                    else if (prop.PropertyType == typeof(string))
                                        prop.SetValue(rel, cellVal);
                                    totalChanges++;
                                }
                            }
                        }
                        if (dataRows.Count > 0)
                            changes["Relationships"] = dataRows.Count;
                        break;
                    case "RESOURCE ASSIGNMENTS":
                        var resProps = ResourcesViewModel.ResourceAssignments.FirstOrDefault()?.GetType().GetProperties();
                        if (resProps == null) continue;
                        foreach (var row in dataRows)
                        {
                            if (row.Length == 0) continue;
                            var idCell = row[0];
                            var res = ResourcesViewModel.ResourceAssignments.FirstOrDefault(r => r.Id.ToString() == idCell);
                            if (res == null) continue;
                            for (int c = 0; c < headerList.Count && c < resProps.Length; c++)
                            {
                                var cellVal = row.Length > c ? row[c] : "";
                                var prop = resProps[c];
                                if (prop.CanWrite && prop.GetValue(res)?.ToString() != cellVal)
                                {
                                    if (prop.PropertyType == typeof(double) && double.TryParse(cellVal, out var dVal))
                                        prop.SetValue(res, dVal);
                                    else if (prop.PropertyType == typeof(DateTime?) && DateTime.TryParse(cellVal, out var dtVal))
                                        prop.SetValue(res, dtVal);
                                    else if (prop.PropertyType == typeof(string))
                                        prop.SetValue(res, cellVal);
                                    totalChanges++;
                                }
                            }
                        }
                        if (dataRows.Count > 0)
                            changes["Resources"] = dataRows.Count;
                        break;
                    case "CALENDARS":
                        var calProps = CalendarsViewModel.Calendars.FirstOrDefault()?.GetType().GetProperties();
                        if (calProps == null) continue;
                        foreach (var row in dataRows)
                        {
                            if (row.Length == 0) continue;
                            var idCell = row[0];
                            var cal = CalendarsViewModel.Calendars.FirstOrDefault(c => c.Id.ToString() == idCell);
                            if (cal == null) continue;
                            for (int c = 0; c < headerList.Count && c < calProps.Length; c++)
                            {
                                var cellVal = row.Length > c ? row[c] : "";
                                var prop = calProps[c];
                                if (prop.CanWrite && prop.GetValue(cal)?.ToString() != cellVal)
                                {
                                    if (prop.PropertyType == typeof(double) && double.TryParse(cellVal, out var dVal))
                                        prop.SetValue(cal, dVal);
                                    else if (prop.PropertyType == typeof(DateTime?) && DateTime.TryParse(cellVal, out var dtVal))
                                        prop.SetValue(cal, dtVal);
                                    else if (prop.PropertyType == typeof(string))
                                        prop.SetValue(cal, cellVal);
                                    totalChanges++;
                                }
                            }
                        }
                        if (dataRows.Count > 0)
                            changes["Calendars"] = dataRows.Count;
                        break;
                    case "CODES":
                        var codeProps = CodesViewModel.Codes.FirstOrDefault()?.GetType().GetProperties();
                        if (codeProps == null) continue;
                        foreach (var row in dataRows)
                        {
                            if (row.Length == 0) continue;
                            var idCell = row[0];
                            var code = CodesViewModel.Codes.FirstOrDefault(c => c.Id.ToString() == idCell);
                            if (code == null) continue;
                            for (int c = 0; c < headerList.Count && c < codeProps.Length; c++)
                            {
                                var cellVal = row.Length > c ? row[c] : "";
                                var prop = codeProps[c];
                                if (prop.CanWrite && prop.GetValue(code)?.ToString() != cellVal)
                                {
                                    if (prop.PropertyType == typeof(double) && double.TryParse(cellVal, out var dVal))
                                        prop.SetValue(code, dVal);
                                    else if (prop.PropertyType == typeof(DateTime?) && DateTime.TryParse(cellVal, out var dtVal))
                                        prop.SetValue(code, dtVal);
                                    else if (prop.PropertyType == typeof(string))
                                        prop.SetValue(code, cellVal);
                                    totalChanges++;
                                }
                            }
                        }
                        if (dataRows.Count > 0)
                            changes["Codes"] = dataRows.Count;
                        break;
                    case "UDFS":
                        var udfProps = UdfsViewModel.Udfs.FirstOrDefault()?.GetType().GetProperties();
                        if (udfProps == null) continue;
                        foreach (var row in dataRows)
                        {
                            if (row.Length == 0) continue;
                            var idCell = row[0];
                            var udf = UdfsViewModel.Udfs.FirstOrDefault(u => u.Id.ToString() == idCell);
                            if (udf == null) continue;
                            for (int c = 0; c < headerList.Count && c < udfProps.Length; c++)
                            {
                                var cellVal = row.Length > c ? row[c] : "";
                                var prop = udfProps[c];
                                if (prop.CanWrite && prop.GetValue(udf)?.ToString() != cellVal)
                                {
                                    if (prop.PropertyType == typeof(double) && double.TryParse(cellVal, out var dVal))
                                        prop.SetValue(udf, dVal);
                                    else if (prop.PropertyType == typeof(DateTime?) && DateTime.TryParse(cellVal, out var dtVal))
                                        prop.SetValue(udf, dtVal);
                                    else if (prop.PropertyType == typeof(string))
                                        prop.SetValue(udf, cellVal);
                                    totalChanges++;
                                }
                            }
                        }
                        if (dataRows.Count > 0)
                            changes["UDFs"] = dataRows.Count;
                        break;
                }
            }

            var detailParts = changes.Select(kv => $"{kv.Key}: {kv.Value} rows updated");
            var detailStr = string.Join(", ", detailParts);

            StatusMessage = totalChanges > 0
                ? $"Imported {totalChanges} change(s) from Excel. ({detailStr})"
                : "No changes detected in Excel file.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Excel import failed: {ex.Message}";
        }
    }

    public PrimaveraCalendarsViewModel CalendarsViewModel { get; }
    public PrimaveraCodesViewModel CodesViewModel { get; }
    public PrimaveraActivitiesViewModel ActivitiesViewModel { get; }
    public PrimaveraRelationshipsViewModel RelationshipsViewModel { get; }
    public PrimaveraResourcesViewModel ResourcesViewModel { get; }
    public PrimaveraBaselinesViewModel BaselinesViewModel { get; }
    public PrimaveraUdfsViewModel UdfsViewModel { get; }
}