namespace Planova.Primavera.Domain.Constants;

public static class XerFieldNames
{
    public const string HeaderRecord = "ERMHDR";
    public const string TableMarker = "%T";
    public const string FieldMarker = "%F";
    public const string RowMarker = "%R";
    public const string EndMarker = "%E";

    public const string CalendarTable = "CALENDAR";
    public const string ProjectTable = "PROJECT";
    public const string TaskTable = "TASK";
    public const string TaskPredTable = "TASKPRED";
    public const string TaskRsrcTable = "TASKRSRC";
    public const string RsProjectTable = "RSPROJECT";
    public const string RsourceTable = "RSOURCE";
    public const string RcatTypeTable = "RCATTYPE";
    public const string RcatValTable = "RCATVAL";
    public const string ProjectCodeTable = "PROJECTCODE";
    public const string ProjCodeCatTable = "PROJCODECAT";
    public const string ProjCodeValTable = "PROJCODEVAL";
    public const string UdfTypeTable = "UDFTYPE";
    public const string UdfValueTable = "UDFVALUE";
    public const string ProjectBaselineTable = "PROJECTBASELINE";
    public const string TaskUdfTable = "TASKUDF";

    public static readonly HashSet<string> SupportedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        CalendarTable, ProjectTable, TaskTable, TaskPredTable, TaskRsrcTable,
        RsProjectTable, RsourceTable, RcatTypeTable, RcatValTable,
        ProjectCodeTable, ProjCodeCatTable, ProjCodeValTable,
        UdfTypeTable, UdfValueTable, ProjectBaselineTable, TaskUdfTable
    };

    public static bool IsSupportedTable(string tableName) =>
        SupportedTables.Contains(tableName);
}
