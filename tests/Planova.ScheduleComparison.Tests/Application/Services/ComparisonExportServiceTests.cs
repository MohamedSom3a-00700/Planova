using ClosedXML.Excel;
using FluentAssertions;
using NSubstitute;
using Planova.ScheduleComparison.Application.Services;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Enums;
using Planova.ScheduleComparison.Domain.Interfaces;
using QuestPDF.Infrastructure;

namespace Planova.ScheduleComparison.Tests.Application.Services;

public class ComparisonExportServiceTests : IDisposable
{
    private readonly IComparisonRepository _repository;
    private readonly ComparisonExportService _service;
    private readonly string _tempDir;

    public ComparisonExportServiceTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _repository = Substitute.For<IComparisonRepository>();
        _service = new ComparisonExportService(_repository);
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task ExportToExcelAsync_GeneratesCorrectWorksheets()
    {
        var sessionId = Guid.NewGuid();
        var session = new ComparisonSession { Id = sessionId };
        var results = new List<ComparisonResult>
        {
            new() { EntityType = "Activity", MatchKey = "A1", ChangeType = ChangeType.Modified, FieldName = "Duration", OldValue = "5", NewValue = "10", Severity = "Minor" },
            new() { EntityType = "Activity", MatchKey = "A2", ChangeType = ChangeType.Added, Severity = "Major" },
            new() { EntityType = "Relationship", MatchKey = "R1", ChangeType = ChangeType.Modified, OldValue = "FS", NewValue = "SS" },
            new() { EntityType = "ResourceAssignment", MatchKey = "Res1", ChangeType = ChangeType.Modified, OldValue = "100", NewValue = "200" },
        };

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(session);
        _repository.GetResultsBySessionIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(results);

        var filePath = await _service.ExportToExcelAsync(sessionId, _tempDir);

        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();

        using var workbook = new XLWorkbook(filePath);
        workbook.Worksheets.Count.Should().Be(3);
        workbook.Worksheet(1).Name.Should().Be("Activities");
        workbook.Worksheet(2).Name.Should().Be("Logic");
        workbook.Worksheet(3).Name.Should().Be("Resources");

        var activitySheet = workbook.Worksheet("Activities");
        Assert.Equal("MatchKey", activitySheet.Cell(1, 1).GetString());
        Assert.Equal("Field", activitySheet.Cell(1, 2).GetString());
        Assert.Equal("Change", activitySheet.Cell(1, 3).GetString());
        Assert.Equal("Old Value", activitySheet.Cell(1, 4).GetString());
        Assert.Equal("New Value", activitySheet.Cell(1, 5).GetString());
        Assert.Equal("Severity", activitySheet.Cell(1, 6).GetString());
        Assert.Equal("A1", activitySheet.Cell(2, 1).GetString());
        Assert.Equal("Duration", activitySheet.Cell(2, 2).GetString());
        Assert.Equal("Modified", activitySheet.Cell(2, 3).GetString());
        Assert.Equal("5", activitySheet.Cell(2, 4).GetString());
        Assert.Equal("10", activitySheet.Cell(2, 5).GetString());
        Assert.Equal("Minor", activitySheet.Cell(2, 6).GetString());
        Assert.Equal("(entity)", activitySheet.Cell(3, 2).GetString());

        var logicSheet = workbook.Worksheet("Logic");
        Assert.Equal("MatchKey", logicSheet.Cell(1, 1).GetString());
        Assert.Equal("Change", logicSheet.Cell(1, 2).GetString());
        Assert.Equal("Old Type", logicSheet.Cell(1, 3).GetString());
        Assert.Equal("New Type", logicSheet.Cell(1, 4).GetString());
        Assert.Equal("R1", logicSheet.Cell(2, 1).GetString());
        Assert.Equal("FS", logicSheet.Cell(2, 3).GetString());
        Assert.Equal("SS", logicSheet.Cell(2, 4).GetString());

        var resourceSheet = workbook.Worksheet("Resources");
        Assert.Equal("MatchKey", resourceSheet.Cell(1, 1).GetString());
        Assert.Equal("Change", resourceSheet.Cell(1, 2).GetString());
        Assert.Equal("Old Units", resourceSheet.Cell(1, 3).GetString());
        Assert.Equal("New Units", resourceSheet.Cell(1, 4).GetString());
        Assert.Equal("Res1", resourceSheet.Cell(2, 1).GetString());
        Assert.Equal("100", resourceSheet.Cell(2, 3).GetString());
        Assert.Equal("200", resourceSheet.Cell(2, 4).GetString());

        // Verify no leftover temp files with the pattern used by the service
        var leftoverTemp = Directory.EnumerateFiles(_tempDir, "_*.xlsx").ToList();
        leftoverTemp.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportToPdfAsync_GeneratesPdfFile()
    {
        var sessionId = Guid.NewGuid();
        var session = new ComparisonSession
        {
            Id = sessionId,
            SourceLabel = "Baseline",
            TargetLabel = "Current",
            CompletedAt = DateTime.UtcNow
        };
        var results = new List<ComparisonResult>();

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(session);
        _repository.GetResultsBySessionIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(results);

        var filePath = await _service.ExportToPdfAsync(sessionId, _tempDir);

        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();
        new FileInfo(filePath).Length.Should().BeGreaterThan(0);

        var header = new byte[5];
        using (var stream = File.OpenRead(filePath))
            await stream.ReadAsync(header, 0, 5);
        System.Text.Encoding.ASCII.GetString(header).Should().Be("%PDF-");
    }

    [Fact]
    public async Task ExportToJsonAsync_GeneratesJsonFileWithCorrectSchema()
    {
        var sessionId = Guid.NewGuid();
        var json = """{"SchemaVersion":"1.0","SessionId":"...","ProjectId":1,"Mode":"UpdateVsUpdate","Summary":{"TotalActivities":0}}""";
        var session = new ComparisonSession { Id = sessionId, ResultJson = json };

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(session);

        var filePath = await _service.ExportToJsonAsync(sessionId, _tempDir);

        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Contain("SchemaVersion");
        content.Should().Contain("SessionId");
        content.Should().Contain("ProjectId");
        content.Should().Contain("Mode");
        content.Should().Contain("Summary");

        Directory.EnumerateFiles(_tempDir, "*.tmp").Should().BeEmpty();
    }

    [Fact]
    public async Task ExportToJsonAsync_WhenNoResultJson_ThrowsInvalidOperationException()
    {
        var sessionId = Guid.NewGuid();
        var session = new ComparisonSession { Id = sessionId, ResultJson = null };

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(session);

        Func<Task> act = () => _service.ExportToJsonAsync(sessionId, _tempDir);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{sessionId}*");
    }

    [Fact]
    public async Task ExportToExcelAsync_WhenSessionNotFound_ThrowsInvalidOperationException()
    {
        var sessionId = Guid.NewGuid();

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns((ComparisonSession?)null);

        Func<Task> act = () => _service.ExportToExcelAsync(sessionId, _tempDir);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{sessionId}*");
    }

    [Fact]
    public async Task ExportToExcelAsync_WhenRepositoryThrows_DoesNotLeaveTempFile()
    {
        var sessionId = Guid.NewGuid();
        var session = new ComparisonSession { Id = sessionId };

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(session);
        _repository.GetResultsBySessionIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<ComparisonResult>>(new InvalidOperationException("Repo failure")));

        Func<Task> act = () => _service.ExportToExcelAsync(sessionId, _tempDir);

        await act.Should().ThrowAsync<InvalidOperationException>();

        Directory.EnumerateFiles(_tempDir, "*.tmp").Should().BeEmpty();
    }
}
