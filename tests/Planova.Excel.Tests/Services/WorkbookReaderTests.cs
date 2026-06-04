using ClosedXML.Excel;
using Planova.Excel.Models;
using Planova.Excel.Readers;
using Planova.Excel.Services;
using Planova.Excel.Tests.Helpers;
using Xunit;

namespace Planova.Excel.Tests.Services;

public class WorkbookReaderTests : IDisposable
{
    private readonly string _testDir;
    private readonly WorkbookReader _reader = new();
    private readonly WorkbookPreviewService _preview;

    public WorkbookReaderTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "Planova_ReaderTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
        _preview = new WorkbookPreviewService(_reader);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public async Task OpenAsync_WithValidWorkbook_ReturnsMetadata()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "test.xlsx", recordCount: 5);

        var info = await _reader.OpenAsync(filePath, CancellationToken.None);

        Assert.True(info.IsValid);
        Assert.Equal(filePath, info.FilePath);
        Assert.True(info.FileSize > 0);
        Assert.Single(info.Worksheets);
        Assert.Equal("Projects", info.Worksheets[0].Name);
        Assert.Equal(6, info.Worksheets[0].RowCount);
        Assert.Equal(4, info.Worksheets[0].ColumnCount);
        Assert.Contains("Code", info.Worksheets[0].Columns);
    }

    [Fact]
    public async Task OpenAsync_WithMultipleSheets_ReturnsAllWorksheets()
    {
        var path = Path.Combine(_testDir, "multi.xlsx");
        using var workbook = new XLWorkbook();
        workbook.Worksheets.Add("Sheet1");
        workbook.Worksheets.Add("Sheet2");
        workbook.Worksheets.Add("Sheet3");
        workbook.SaveAs(path);

        var info = await _reader.OpenAsync(path, CancellationToken.None);

        Assert.Equal(3, info.Worksheets.Count);
        Assert.Contains(info.Worksheets, w => w.Name == "Sheet1");
        Assert.Contains(info.Worksheets, w => w.Name == "Sheet2");
        Assert.Contains(info.Worksheets, w => w.Name == "Sheet3");
    }

    [Fact]
    public async Task GetWorksheetInfoAsync_ReturnsCorrectInfo()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "ws.xlsx", recordCount: 8);

        var info = await _reader.GetWorksheetInfoAsync(filePath, "Projects", CancellationToken.None);

        Assert.Equal("Projects", info.Name);
        Assert.Equal(9, info.RowCount);
        Assert.Equal(4, info.ColumnCount);
        Assert.Contains("Code", info.Columns);
    }

    [Fact]
    public async Task PreviewAsync_FirstPage_ReturnsFirstPageData()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "pages.xlsx", recordCount: 20);

        var preview = await _reader.PreviewAsync(filePath, "Projects", page: 1, pageSize: 5, CancellationToken.None);

        Assert.Equal(5, preview.Rows.Count);
        Assert.Equal(20, preview.TotalRowCount);
        Assert.Equal(1, preview.CurrentPage);
        Assert.Equal(5, preview.PageSize);
        Assert.Equal("P001", preview.Rows[0]["Code"]?.ToString());
        Assert.Equal("P005", preview.Rows[4]["Code"]?.ToString());
    }

    [Fact]
    public async Task PreviewAsync_SecondPage_ReturnsNextPageData()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "pages2.xlsx", recordCount: 20);

        var preview = await _reader.PreviewAsync(filePath, "Projects", page: 2, pageSize: 5, CancellationToken.None);

        Assert.Equal(5, preview.Rows.Count);
        Assert.Equal(20, preview.TotalRowCount);
        Assert.Equal(2, preview.CurrentPage);
        Assert.Equal("P006", preview.Rows[0]["Code"]?.ToString());
    }

    [Fact]
    public async Task PreviewAsync_LastPage_PartialPage()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "partial.xlsx", recordCount: 7);

        var preview = await _reader.PreviewAsync(filePath, "Projects", page: 2, pageSize: 5, CancellationToken.None);

        Assert.Equal(2, preview.Rows.Count);
        Assert.Equal(7, preview.TotalRowCount);
    }

    [Fact]
    public async Task ReadAllAsync_ReturnsAllRecords()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "all.xlsx", recordCount: 15);

        var records = await _reader.ReadAllAsync(filePath, "Projects", CancellationToken.None);

        Assert.Equal(15, records.Count);
        Assert.Equal("P001", records[0]["Code"]?.ToString());
        Assert.Equal("P015", records[14]["Code"]?.ToString());
    }

    [Fact]
    public void CanRead_SupportedExtensions_ReturnsTrue()
    {
        Assert.True(_reader.CanRead("data.xlsx"));
        Assert.True(_reader.CanRead("data.xlsm"));
    }

    [Fact]
    public void CanRead_UnsupportedExtensions_ReturnsFalse()
    {
        Assert.False(_reader.CanRead("data.csv"));
        Assert.False(_reader.CanRead("data.xls"));
        Assert.False(_reader.CanRead("data.pdf"));
        Assert.False(_reader.CanRead(""));
    }

    [Fact]
    public async Task OpenAsync_WithEmptyWorkbook_Succeeds()
    {
        var filePath = TestWorkbookHelper.CreateEmptyWorkbook(_testDir, "empty.xlsx");

        var info = await _reader.OpenAsync(filePath, CancellationToken.None);

        Assert.True(info.IsValid);
        Assert.True(info.FileSize >= 0);
    }

    [Fact]
    public async Task OpenAsync_WithEmptyFile_Throws()
    {
        var filePath = TestWorkbookHelper.CreateEmptyFile(_testDir, "empty.xlsx");

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _reader.OpenAsync(filePath, CancellationToken.None));
    }

    [Fact]
    public async Task OpenAsync_FileNotFound_Throws()
    {
        await Assert.ThrowsAnyAsync<FileNotFoundException>(() =>
            _reader.OpenAsync(Path.Combine(_testDir, "missing.xlsx"), CancellationToken.None));
    }

    [Fact]
    public async Task OpenAsync_UnsupportedExtension_Throws()
    {
        var filePath = TestWorkbookHelper.CreateEmptyFile(_testDir, "data.csv");

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _reader.OpenAsync(filePath, CancellationToken.None));
    }

    [Fact]
    public async Task OpenAsync_NullPath_Throws()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _reader.OpenAsync("", CancellationToken.None));
    }

    [Fact]
    public async Task PreviewService_GetWorkbookInfoAsync_ReturnsInfo()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "svc.xlsx", recordCount: 3);

        var info = await _preview.GetWorkbookInfoAsync(filePath, CancellationToken.None);

        Assert.True(info.IsValid);
        Assert.Single(info.Worksheets);
    }

    [Fact]
    public async Task PreviewService_GetPreviewAsync_PaginatesCorrectly()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "svc_pages.xlsx", recordCount: 12);

        var preview = await _preview.GetPreviewAsync(filePath, "Projects", page: 2, pageSize: 5, CancellationToken.None);

        Assert.Equal(5, preview.Rows.Count);
        Assert.Equal(2, preview.CurrentPage);
    }

    [Fact]
    public async Task PreviewService_SearchAsync_FindsMatchingSheets()
    {
        var path = Path.Combine(_testDir, "search.xlsx");
        using var workbook = new XLWorkbook();
        workbook.Worksheets.Add("Projects");
        workbook.Worksheets.Add("Activities");
        workbook.Worksheets.Add("Resources");
        workbook.SaveAs(path);

        var results = new List<string>();
        await foreach (var name in _preview.SearchAsync(path, "Projects", CancellationToken.None))
        {
            results.Add(name);
        }

        Assert.Contains("Projects", results);
        Assert.DoesNotContain("Activities", results);
    }

    [Fact]
    public async Task ReadAllAsync_WithEmptyWorksheet_ReturnsEmptyList()
    {
        var path = Path.Combine(_testDir, "emptyws.xlsx");
        using var workbook = new XLWorkbook();
        workbook.Worksheets.Add("EmptySheet");
        workbook.SaveAs(path);

        var records = await _reader.ReadAllAsync(path, "EmptySheet", CancellationToken.None);

        Assert.Empty(records);
    }
}
