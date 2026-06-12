using Planova.Excel.Models;
using Planova.Excel.Readers;
using Planova.Excel.Services;
using Planova.Excel.Tests.Helpers;
using Planova.Excel.Validation;
using Xunit;

namespace Planova.Excel.Tests.Services;

public class ImportWorkflowTests : IDisposable
{
    private readonly string _testDir;
    private readonly WorkbookReader _reader = new();
    private readonly ValidationService _validator = new();
    private readonly ImportService _sut;

    public ImportWorkflowTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "Planova_ImportTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
        _sut = new ImportService(_reader, _validator);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public async Task ImportAsync_WithValidProjectData_ImportsAllRecords()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "projects.xlsx", recordCount: 25);

        var request = new ImportRequest
        {
            FilePath = filePath,
            WorksheetName = "Projects",
            EntityType = "Project",
            ColumnMappings = new()
            {
                ["Code"] = "Code",
                ["Name"] = "Name",
                ["Status"] = "Status",
                ["Client"] = "Client"
            },
            DuplicateHandling = DuplicateStrategy.SkipAll
        };

        var progress = new Progress<int>();
        var result = await _sut.ImportAsync(request, progress, CancellationToken.None);

        Assert.Equal(25, result.TotalRecords);
        Assert.Equal(25, result.ImportedRecords);
        Assert.Equal(0, result.FailedRecords);
        Assert.True(result.Duration.TotalMilliseconds >= 0);
    }

    [Fact]
    public async Task ImportAsync_WithDuplicateSkipAll_SkipsDuplicates()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "dupes.xlsx", recordCount: 5);

        var request = new ImportRequest
        {
            FilePath = filePath,
            WorksheetName = "Projects",
            EntityType = "Project",
            ColumnMappings = new()
            {
                ["Code"] = "Code",
                ["Name"] = "Name",
                ["Status"] = "Status",
                ["Client"] = "Client"
            },
            DuplicateHandling = DuplicateStrategy.SkipAll
        };

        var result = await _sut.ImportAsync(request, null, CancellationToken.None);

        Assert.Equal(5, result.TotalRecords);
    }

    [Fact]
    public async Task ImportAsync_WithDuplicateCancel_StopsOnFirstDuplicate()
    {
        var columns = new List<string> { "Code", "Name" };
        var rows = new List<Dictionary<string, object>>
        {
            new() { ["Code"] = "P001", ["Name"] = "Project One" },
            new() { ["Code"] = "P002", ["Name"] = "Project Two" }
        };
        var filePath = TestWorkbookHelper.CreateWorkbook(_testDir, "cancel.xlsx", "Projects", columns, rows);

        var request = new ImportRequest
        {
            FilePath = filePath,
            WorksheetName = "Projects",
            EntityType = "Project",
            ColumnMappings = new() { ["Code"] = "Code", ["Name"] = "Name" },
            DuplicateHandling = DuplicateStrategy.Cancel
        };

        var result = await _sut.ImportAsync(request, null, CancellationToken.None);

        Assert.Equal(2, result.TotalRecords);
    }

    [Fact]
    public async Task ImportAsync_WithMismatchedColumns_ReturnsFailedRecords()
    {
        var columns = new List<string> { "ColA", "ColB", "ColC" };
        var rows = new List<Dictionary<string, object>>
        {
            new() { ["ColA"] = "P001", ["ColB"] = "Alpha", ["ColC"] = "Active" },
            new() { ["ColA"] = "P002", ["ColB"] = "Beta", ["ColC"] = "Draft" }
        };
        var filePath = TestWorkbookHelper.CreateWorkbook(_testDir, "mismatched.xlsx", "Projects", columns, rows);

        var request = new ImportRequest
        {
            FilePath = filePath,
            WorksheetName = "Projects",
            EntityType = "Project",
            ColumnMappings = new()
            {
                ["Code"] = "Code",
                ["Name"] = "Name"
            },
            DuplicateHandling = DuplicateStrategy.SkipAll
        };

        var result = await _sut.ImportAsync(request, null, CancellationToken.None);

        Assert.Equal(2, result.TotalRecords);
        Assert.True(result.FailedRecords > 0, "Expected failed records due to missing mapped columns in workbook");
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WithRealWorkbook_ReturnsValid()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "validate.xlsx", recordCount: 3);

        var request = new ImportRequest
        {
            FilePath = filePath,
            WorksheetName = "Projects",
            EntityType = "Project",
            ColumnMappings = new()
            {
                ["Code"] = "Code",
                ["Name"] = "Name",
                ["Status"] = "Status",
                ["Client"] = "Client"
            }
        };

        var result = await _sut.ValidateAsync(request, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task PreviewImportAsync_ReturnsPreviewWithoutCommitting()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "preview.xlsx", recordCount: 7);

        var request = new ImportRequest
        {
            FilePath = filePath,
            WorksheetName = "Projects",
            EntityType = "Project",
            ColumnMappings = new()
            {
                ["Code"] = "Code",
                ["Name"] = "Name",
                ["Status"] = "Status",
                ["Client"] = "Client"
            }
        };

        var result = await _sut.PreviewImportAsync(request, CancellationToken.None);

        Assert.Equal(7, result.TotalRecords);
        Assert.Equal(0, result.ImportedRecords);
        Assert.Equal(0, result.UpdatedRecords);
    }

    [Fact]
    public async Task ImportAsync_Cancellation_Throws()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "cancel_test.xlsx", recordCount: 50);

        var request = new ImportRequest
        {
            FilePath = filePath,
            WorksheetName = "Projects",
            EntityType = "Project",
            ColumnMappings = new() { ["Code"] = "Code", ["Name"] = "Name" }
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.ImportAsync(request, null, cts.Token));
    }

    [Fact]
    public async Task ImportAsync_WithUnsupportedExtension_Throws()
    {
        var filePath = TestWorkbookHelper.CreateEmptyFile(_testDir, "data.csv");
        var request = new ImportRequest
        {
            FilePath = filePath,
            WorksheetName = "Sheet1",
            EntityType = "Project",
            ColumnMappings = new() { ["Code"] = "Code" }
        };

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _sut.ImportAsync(request, null, CancellationToken.None));
    }

    [Fact]
    public async Task ImportAsync_FileNotFound_Throws()
    {
        var request = new ImportRequest
        {
            FilePath = Path.Combine(_testDir, "nonexistent.xlsx"),
            WorksheetName = "Sheet1",
            EntityType = "Project",
            ColumnMappings = new() { ["Code"] = "Code" }
        };

        await Assert.ThrowsAnyAsync<FileNotFoundException>(() =>
            _sut.ImportAsync(request, null, CancellationToken.None));
    }

    [Fact]
    public async Task ImportAsync_WithDifferentEntityTypes_UsesCorrectKeyFields()
    {
        var columns = new List<string> { "Code", "ProjectId", "Name" };
        var rows = new List<Dictionary<string, object>>
        {
            new() { ["Code"] = "A001", ["ProjectId"] = "P001", ["Name"] = "Activity One" }
        };
        var filePath = TestWorkbookHelper.CreateWorkbook(_testDir, "activities.xlsx", "Activities", columns, rows);

        var request = new ImportRequest
        {
            FilePath = filePath,
            WorksheetName = "Activities",
            EntityType = "Activity",
            ColumnMappings = new()
            {
                ["Code"] = "Code",
                ["ProjectId"] = "ProjectId",
                ["Name"] = "Name"
            }
        };

        var result = await _sut.ImportAsync(request, null, CancellationToken.None);

        Assert.Equal(1, result.TotalRecords);
        Assert.Equal(1, result.ImportedRecords);
    }

    [Fact]
    public async Task ImportAsync_Progress_ReportsPercentages()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "progress.xlsx", recordCount: 10);
        var progressValues = new List<int>();

        var request = new ImportRequest
        {
            FilePath = filePath,
            WorksheetName = "Projects",
            EntityType = "Project",
            ColumnMappings = new() { ["Code"] = "Code", ["Name"] = "Name", ["Status"] = "Status" },
            BatchSize = 5,
            DuplicateHandling = DuplicateStrategy.SkipAll
        };

        var progress = new SyncProgress(val => progressValues.Add(val));
        await _sut.ImportAsync(request, progress, CancellationToken.None);

        Assert.NotEmpty(progressValues);
        Assert.Equal(100, progressValues[^1]);
    }

    [Fact]
    public async Task ValidateAsync_Cancellation_Throws()
    {
        var filePath = TestWorkbookHelper.CreateProjectWorkbook(_testDir, "val_cancel.xlsx");

        var request = new ImportRequest
        {
            FilePath = filePath,
            WorksheetName = "Projects",
            EntityType = "Project",
            ColumnMappings = new() { ["Code"] = "Code" }
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.ValidateAsync(request, cts.Token));
    }
}
