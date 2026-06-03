using ClosedXML.Excel;
using Planova.Excel.Models;
using Planova.Excel.Readers;
using Planova.Excel.Services;
using Planova.Excel.Writers;
using Xunit;

namespace Planova.Excel.Tests.Services;

public class ExportWorkflowTests : IDisposable
{
    private readonly string _testDir;
    private readonly WorkbookWriter _writer = new();
    private readonly WorkbookReader _reader = new();
    private readonly ExportService _sut;

    public ExportWorkflowTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "Planova_ExportTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
        _sut = new ExportService(_writer);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public async Task WriteThenRead_WithProjectData_VerifiesContent()
    {
        var outputPath = Path.Combine(_testDir, "projects.xlsx");
        var data = new List<Dictionary<string, object>>
        {
            new() { ["Code"] = "P001", ["Name"] = "Alpha", ["Status"] = "Active" },
            new() { ["Code"] = "P002", ["Name"] = "Beta", ["Status"] = "Draft" }
        };

        var request = new ExportRequest
        {
            SelectedColumns = new() { "Code", "Name", "Status" },
            OutputPath = outputPath,
            SheetName = "Projects",
            IncludeHeaders = true
        };

        var result = await _writer.WriteAsync(request, data, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.TotalRecords);
        Assert.True(result.FileSize > 0);

        using var workbook = new XLWorkbook(outputPath);
        var ws = workbook.Worksheet(1);
        Assert.Equal("Code", ws.Cell(1, 1).GetString());
        Assert.Equal("P001", ws.Cell(2, 1).GetString());
        Assert.Equal("Beta", ws.Cell(3, 2).GetString());
    }

    [Fact]
    public async Task WriteThenRead_WithoutHeaders_OmitsHeaderRow()
    {
        var outputPath = Path.Combine(_testDir, "noheaders.xlsx");
        var data = new List<Dictionary<string, object>>
        {
            new() { ["Code"] = "P001", ["Name"] = "Alpha" }
        };

        var request = new ExportRequest
        {
            SelectedColumns = new() { "Code", "Name" },
            OutputPath = outputPath,
            SheetName = "Projects",
            IncludeHeaders = false
        };

        var result = await _writer.WriteAsync(request, data, CancellationToken.None);

        Assert.True(result.Success);
        using var workbook = new XLWorkbook(outputPath);
        var ws = workbook.Worksheet(1);
        Assert.Equal("P001", ws.Cell(1, 1).GetString());
        Assert.Equal("Alpha", ws.Cell(1, 2).GetString());
    }

    [Fact]
    public async Task WriteThenRead_WithAllDataTypes_HandlesTypeConversion()
    {
        var outputPath = Path.Combine(_testDir, "types.xlsx");
        var now = new DateTime(2026, 6, 3, 12, 0, 0);
        var data = new List<Dictionary<string, object>>
        {
            new()
            {
                ["Name"] = "Test",
                ["Count"] = 42,
                ["Rate"] = 99.5,
                ["IsActive"] = true,
                ["StartDate"] = now
            }
        };

        var request = new ExportRequest
        {
            SelectedColumns = new() { "Name", "Count", "Rate", "IsActive", "StartDate" },
            OutputPath = outputPath,
            SheetName = "Resources",
            IncludeHeaders = true
        };

        var result = await _writer.WriteAsync(request, data, CancellationToken.None);

        Assert.True(result.Success);
        using var workbook = new XLWorkbook(outputPath);
        var ws = workbook.Worksheet(1);
        Assert.Equal("Test", ws.Cell(2, 1).GetString());
        Assert.Equal(42, (int)ws.Cell(2, 2).GetDouble());
        Assert.Equal(99.5, ws.Cell(2, 3).GetDouble());
        Assert.True(ws.Cell(2, 4).GetBoolean());
        Assert.Equal(now, ws.Cell(2, 5).GetDateTime());
    }

    [Fact]
    public async Task WriteThenRead_WithEmptyData_WritesOnlyHeaders()
    {
        var outputPath = Path.Combine(_testDir, "empty.xlsx");
        var data = new List<Dictionary<string, object>>();

        var request = new ExportRequest
        {
            SelectedColumns = new() { "Code", "Name" },
            OutputPath = outputPath,
            SheetName = "Projects",
            IncludeHeaders = true
        };

        var result = await _writer.WriteAsync(request, data, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(0, result.TotalRecords);
        using var workbook = new XLWorkbook(outputPath);
        var ws = workbook.Worksheet(1);
        Assert.Equal("Code", ws.Cell(1, 1).GetString());
        Assert.Equal("Name", ws.Cell(1, 2).GetString());
    }

    [Fact]
    public async Task ExportService_Cancellation_Throws()
    {
        var outputPath = Path.Combine(_testDir, "cancel.xlsx");

        var request = new ExportRequest
        {
            EntityType = "Project",
            SelectedColumns = new() { "Code" },
            OutputPath = outputPath,
            SheetName = "Projects"
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.ExportAsync(request, null, cts.Token));
    }

    [Fact]
    public async Task BuildRequestAsync_ReturnsDefaultColumnsForEntity()
    {
        var request = await _sut.BuildRequestAsync("Project", CancellationToken.None);

        Assert.Equal("Project", request.EntityType);
        Assert.Equal("Project", request.SheetName);
        Assert.True(request.IncludeHeaders);
        Assert.Contains("Code", request.SelectedColumns);
        Assert.Contains("Name", request.SelectedColumns);
        Assert.Contains("Status", request.SelectedColumns);
    }

    [Fact]
    public async Task BuildRequestAsync_ForActivity_ReturnsActivityColumns()
    {
        var request = await _sut.BuildRequestAsync("Activity", CancellationToken.None);

        Assert.Contains("Code", request.SelectedColumns);
        Assert.Contains("Duration", request.SelectedColumns);
    }

    [Fact]
    public void GetExportableEntityTypes_ReturnsKnownTypes()
    {
        var types = _sut.GetExportableEntityTypes();

        Assert.Contains("Project", types);
        Assert.Contains("Activity", types);
        Assert.Contains("Resource", types);
        Assert.Contains("Cost", types);
        Assert.Contains("Risk", types);
    }

    [Fact]
    public async Task WriteThenRead_WithNestedDirectory_CreatesDirectory()
    {
        var nestedDir = Path.Combine(_testDir, "subdir", "nested");
        var outputPath = Path.Combine(nestedDir, "export.xlsx");
        var data = new List<Dictionary<string, object>>
        {
            new() { ["Code"] = "P001" }
        };

        var request = new ExportRequest
        {
            SelectedColumns = new() { "Code" },
            OutputPath = outputPath,
            SheetName = "Projects"
        };

        var result = await _writer.WriteAsync(request, data, CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public void WorkbookWriter_CanWrite_ReturnsTrueForXlsx()
    {
        Assert.True(_writer.CanWrite(".xlsx"));
        Assert.True(_writer.CanWrite("xlsx"));
        Assert.False(_writer.CanWrite(".csv"));
        Assert.False(_writer.CanWrite(".pdf"));
    }

    [Theory]
    [InlineData("P001", "Project Alpha", "Active")]
    [InlineData("P002", "Project Beta", "Draft")]
    public async Task WriteThenRead_RoundTrip_ValuesMatch(string code, string name, string status)
    {
        var outputPath = Path.Combine(_testDir, "roundtrip.xlsx");
        var data = new List<Dictionary<string, object>>
        {
            new() { ["Code"] = code, ["Name"] = name, ["Status"] = status }
        };

        var request = new ExportRequest
        {
            SelectedColumns = new() { "Code", "Name", "Status" },
            OutputPath = outputPath,
            SheetName = "Projects"
        };

        var exportResult = await _writer.WriteAsync(request, data, CancellationToken.None);
        Assert.True(exportResult.Success);

        var preview = await _reader.PreviewAsync(outputPath, "Projects", 1, 100, CancellationToken.None);
        Assert.Equal(1, preview.TotalRowCount);
        Assert.Equal(code, preview.Rows[0]["Code"]?.ToString());
        Assert.Equal(name, preview.Rows[0]["Name"]?.ToString());
        Assert.Equal(status, preview.Rows[0]["Status"]?.ToString());
    }
}
