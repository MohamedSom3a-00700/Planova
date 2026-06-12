using System.IO.Compression;
using System.Xml.Linq;
using ClosedXML.Excel;
using Planova.Excel.Readers;
using Xunit;

namespace Planova.Excel.Tests.Services;

public class WorkbookReaderRepairTests : IDisposable
{
    private readonly string _testDir;
    private readonly WorkbookReader _reader = new();

    public WorkbookReaderRepairTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "Planova_Repair_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public async Task OpenWorkbook_WithFormulas_OpensNormally()
    {
        var filePath = CreateFormulaWorkbook("formulas.xlsx");

        var info = await _reader.OpenAsync(filePath, CancellationToken.None);

        Assert.True(info.IsValid);
        Assert.Single(info.Worksheets);
        Assert.Contains("Total", info.Worksheets[0].Columns);
    }

    [Fact]
    public async Task OpenWorkbook_WithExternalDefinedNames_UsesRepairFallback()
    {
        var filePath = CreateFormulaWorkbook("ext-refs.xlsx");
        AddExternalDefinedName(filePath, "Notes", "'[42]BOQ'!XFC1047991");

        var info = await _reader.OpenAsync(filePath, CancellationToken.None);

        Assert.True(info.IsValid);
        Assert.Single(info.Worksheets);
        Assert.Contains("Item", info.Worksheets[0].Columns);
        Assert.Contains("Total", info.Worksheets[0].Columns);
    }

    [Fact]
    public async Task OpenWorkbook_WithMultipleExternalDefinedNames_UsesRepairFallback()
    {
        var filePath = CreateFormulaWorkbook("multi-ext.xlsx");
        AddExternalDefinedName(filePath, "ExtLink1", "'[1]Sheet1'!$A$1");
        AddExternalDefinedName(filePath, "ExtLink2", "'[2]Data'!$B$2:$D$10");

        var info = await _reader.OpenAsync(filePath, CancellationToken.None);

        Assert.True(info.IsValid);
        Assert.Single(info.Worksheets);
    }

    [Fact]
    public async Task PreviewAsync_WithExternalDefinedNames_UsesRepairFallback()
    {
        var filePath = CreateFormulaWorkbook("preview-ext.xlsx");
        AddExternalDefinedName(filePath, "Notes", "'[42]BOQ'!XFC1047991");

        var preview = await _reader.PreviewAsync(filePath, "Sheet1", 1, 10, CancellationToken.None);

        Assert.Equal("Sheet1", preview.WorksheetName);
        Assert.Equal(3, preview.TotalRowCount);
        Assert.Equal(3, preview.Rows.Count);
    }

    private string CreateFormulaWorkbook(string fileName)
    {
        var path = Path.Combine(_testDir, fileName);
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Sheet1");

        ws.Cell(1, 1).Value = "Item";
        ws.Cell(1, 2).Value = "Qty";
        ws.Cell(1, 3).Value = "Rate";
        ws.Cell(1, 4).Value = "Total";

        ws.Cell(2, 1).Value = "Widget A";
        ws.Cell(2, 2).Value = 10;
        ws.Cell(2, 3).Value = 5.0;
        ws.Cell(2, 4).FormulaA1 = "=B2*C2";

        ws.Cell(3, 1).Value = "Widget B";
        ws.Cell(3, 2).Value = 20;
        ws.Cell(3, 3).Value = 7.5;
        ws.Cell(3, 4).FormulaA1 = "=B3*C3";

        ws.Cell(4, 1).Value = "Widget C";
        ws.Cell(4, 2).Value = 15;
        ws.Cell(4, 3).Value = 3.0;
        ws.Cell(4, 4).FormulaA1 = "=B4*C4";

        workbook.SaveAs(path);
        return path;
    }

    private static void AddExternalDefinedName(string filePath, string name, string formula)
    {
        var tempPath = filePath + ".tmp";
        using (var archive = ZipFile.Open(filePath, ZipArchiveMode.Read))
        using (var output = ZipFile.Open(tempPath, ZipArchiveMode.Create))
        {
            foreach (var entry in archive.Entries)
            {
                var isWorkbook = entry.FullName.Equals("xl/workbook.xml", StringComparison.OrdinalIgnoreCase);
                using var entryStream = entry.Open();
                var outputEntry = output.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                using var outputStream = outputEntry.Open();
                if (isWorkbook)
                {
                    using var reader = new StreamReader(entryStream);
                    var xml = reader.ReadToEnd();
                    var doc = XDocument.Parse(xml);
                    XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

                    var definedNames = doc.Descendants(ns + "definedNames").FirstOrDefault();
                    if (definedNames is null)
                    {
                        var workbookEl = doc.Root!;
                        definedNames = new XElement(ns + "definedNames");
                        var sheets = workbookEl.Element(ns + "sheets");
                        if (sheets is not null)
                            sheets.AddAfterSelf(definedNames);
                        else
                            workbookEl.Add(definedNames);
                    }

                    definedNames.Add(new XElement(ns + "definedName",
                        new XAttribute("name", name),
                        formula));

                    using var writer = new StreamWriter(outputStream);
                    writer.Write(doc.ToString());
                }
                else
                {
                    entryStream.CopyTo(outputStream);
                }
            }
        }
        File.Delete(filePath);
        File.Move(tempPath, filePath);
    }
}
