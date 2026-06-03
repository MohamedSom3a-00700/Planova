using ClosedXML.Excel;

namespace Planova.Excel.Tests.Helpers;

public static class TestWorkbookHelper
{
    public static string CreateWorkbook(string directory, string fileName, string sheetName,
        List<string> columns, List<Dictionary<string, object>> rows)
    {
        var path = Path.Combine(directory, fileName);
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(sheetName);

        for (int i = 0; i < columns.Count; i++)
            ws.Cell(1, i + 1).Value = columns[i];

        for (int r = 0; r < rows.Count; r++)
        {
            for (int c = 0; c < columns.Count; c++)
            {
                if (rows[r].TryGetValue(columns[c], out var val) && val is not null)
                    ws.Cell(r + 2, c + 1).Value = ConvertToXLCellValue(val);
            }
        }

        workbook.SaveAs(path);
        return path;
    }

    public static string CreateProjectWorkbook(string directory, string fileName, int recordCount = 10)
    {
        var columns = new List<string> { "Code", "Name", "Status", "Client" };
        var rows = new List<Dictionary<string, object>>();
        for (int i = 1; i <= recordCount; i++)
        {
            rows.Add(new()
            {
                ["Code"] = $"P{i:D3}",
                ["Name"] = $"Project {i}",
                ["Status"] = i % 2 == 0 ? "Active" : "Draft",
                ["Client"] = $"Client {i % 3 + 1}"
            });
        }
        return CreateWorkbook(directory, fileName, "Projects", columns, rows);
    }

    public static string CreateEmptyFile(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, "");
        return path;
    }

    public static string CreateEmptyWorkbook(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        using var workbook = new XLWorkbook();
        workbook.Worksheets.Add("Sheet1");
        workbook.SaveAs(path);
        return path;
    }

    private static XLCellValue ConvertToXLCellValue(object value)
    {
        return value switch
        {
            string s => s,
            int i => i,
            long l => l,
            double d => d,
            decimal m => (double)m,
            bool b => b,
            DateTime dt => dt,
            _ => value.ToString() ?? string.Empty
        };
    }
}
