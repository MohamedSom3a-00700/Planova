using ClosedXML.Excel;
using Moq;
using Planova.Boq.Application.Dto;
using Planova.Boq.Application.Services;
using Planova.Boq.CsvReader;
using Planova.Boq.Domain.Interfaces;
using Planova.Excel.Readers;
using Xunit;

namespace Planova.Boq.Tests.Services;

public class BoqImportSmokeTests : IDisposable
{
    private readonly string _testDir;
    private readonly WorkbookReader _reader = new();

    public BoqImportSmokeTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "Planova_BoqSmoke_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public async Task OpenBoqWorkbook_DetectsWorksheets()
    {
        var filePath = CreateBoqWorkbook("BOQ Sample", 10);

        var info = await _reader.OpenAsync(filePath, CancellationToken.None);

        Assert.True(info.IsValid);
        Assert.Single(info.Worksheets);
        Assert.Equal("BOQ", info.Worksheets[0].Name);
        Assert.Contains("Code", info.Worksheets[0].Columns);
        Assert.Contains("Description", info.Worksheets[0].Columns);
    }

    [Fact]
    public async Task OpenBoqWorkbook_GetWorksheetInfo_ReturnsColumns()
    {
        var filePath = CreateBoqWorkbook("BOQ Sample", 5);

        var info = await _reader.GetWorksheetInfoAsync(filePath, "BOQ", CancellationToken.None);

        Assert.Equal("BOQ", info.Name);
        Assert.Equal(6, info.RowCount);
        Assert.Equal(5, info.ColumnCount);
        Assert.Contains("Code", info.Columns);
        Assert.Contains("Rate", info.Columns);
    }

    [Fact]
    public async Task ReadBoqWorkbook_ReturnsAllRows()
    {
        var filePath = CreateBoqWorkbook("BOQ Sample", 25);

        var records = await _reader.ReadAllAsync(filePath, "BOQ", CancellationToken.None);

        Assert.Equal(25, records.Count);
        Assert.Equal("BQ001", records[0]["Code"]?.ToString());
        Assert.Equal("BQ025", records[24]["Code"]?.ToString());
    }

    [Fact]
    public async Task ReadBoqWorkbook_ValuesAreCorrect()
    {
        var filePath = CreateBoqWorkbook("BOQ Sample", 3);

        var records = await _reader.ReadAllAsync(filePath, "BOQ", CancellationToken.None);

        Assert.Equal(3, records.Count);
        Assert.Equal("BQ001", records[0]["Code"]);
        Assert.Equal("Mobilization", records[0]["Description"]);
        Assert.Equal("LS", records[0]["Unit"]);
        Assert.Equal(10, records[0]["Quantity"]);
        Assert.Equal(1000, records[0]["Rate"]);
    }

    [Fact]
    public async Task OpenWorkbook_ParsesAllValidWorksheets()
    {
        var filePath = Path.Combine(_testDir, "corrupt.xlsx");
        using (var workbook = new XLWorkbook())
        {
            var ws = workbook.Worksheets.Add("Good");
            ws.Cell(1, 1).Value = "Code";
            ws.Cell(2, 1).Value = "BQ001";
            var badWs = workbook.Worksheets.Add("Bad");
            badWs.Cell(1, 1).Value = 42;
            workbook.SaveAs(filePath);
        }

        var info = await _reader.OpenAsync(filePath, CancellationToken.None);

        Assert.True(info.IsValid);
        Assert.Equal(2, info.Worksheets.Count);
        Assert.NotNull(info.Worksheets[0].Columns);
    }

    [Fact]
    public async Task PreviewImport_WithBoqData_ReturnsPreview()
    {
        var filePath = CreateBoqWorkbook("BOQ Preview", 10);
        var records = await _reader.ReadAllAsync(filePath, "BOQ", CancellationToken.None);

        var importRows = records.Select(r => new ImportRow(
            Code: r["Code"]?.ToString(),
            Description: r["Description"]?.ToString(),
            Unit: r["Unit"]?.ToString() ?? "EA",
            Quantity: GetDecimal(r["Quantity"]),
            Rate: GetDecimal(r["Rate"]),
            Level: null,
            ParentId: null,
            ParentCode: null,
            RawValues: new Dictionary<string, object>(r, StringComparer.OrdinalIgnoreCase)
        )).ToList();

        var treeBuilder = new TreeBuilderService();
        var service = new BoqImportService(
            Mock.Of<IBoqRepository>(),
            Mock.Of<IBoqItemRepository>(),
            treeBuilder,
            Mock.Of<IBoqCsvReader>(),
            Mock.Of<IExcelRowReader>());

        var preview = await service.PreviewImportAsync(importRows, TreeBuildStrategy.LevelColumn, CancellationToken.None);

        Assert.NotNull(preview);
        Assert.Equal(10, preview.TotalItems);
    }

    private string CreateBoqWorkbook(string name, int itemCount)
    {
        var path = Path.Combine(_testDir, $"{name}.xlsx");
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("BOQ");

        ws.Cell(1, 1).Value = "Code";
        ws.Cell(1, 2).Value = "Description";
        ws.Cell(1, 3).Value = "Unit";
        ws.Cell(1, 4).Value = "Quantity";
        ws.Cell(1, 5).Value = "Rate";

        for (int i = 1; i <= itemCount; i++)
        {
            ws.Cell(i + 1, 1).Value = $"BQ{i:D3}";
            ws.Cell(i + 1, 2).Value = i switch
            {
                1 => "Mobilization",
                2 => "Site Preparation",
                3 => "Excavation",
                4 => "Concrete Works",
                5 => "Steel Reinforcement",
                6 => "Formwork",
                7 => "Plumbing",
                8 => "Electrical",
                9 => "Finishing",
                _ => $"Work Item {i}"
            };
            ws.Cell(i + 1, 3).Value = i switch
            {
                1 => "LS",
                2 => "M2",
                _ => "EA"
            };
            ws.Cell(i + 1, 4).Value = 10.0 * i;
            ws.Cell(i + 1, 5).Value = 1000.0 * i;
        }

        workbook.SaveAs(path);
        return path;
    }

    private static decimal GetDecimal(object? val) => val switch
    {
        double d => (decimal)d,
        int i => i,
        decimal m => m,
        string s when decimal.TryParse(s, out var p) => p,
        _ => 0m
    };
}
