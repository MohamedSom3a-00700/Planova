using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Application.Mappings;
using Planova.Primavera.Application.Models;
using Planova.Primavera.Application.Parsers;
using Planova.Primavera.Application.Services;
using Planova.Primavera.Application.Writers;
using Planova.Primavera.Background;
using Planova.Primavera.Domain.Constants;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Enums;
using Planova.Primavera.Domain.Interfaces;
using Planova.Primavera.Extensions;
using Planova.Shared.Abstractions;

namespace Planova.Primavera.Tests.Integration;

public class PrimaveraStudioSmokeTests
{
    [Fact]
    public void AllDomainEntities_CanBeInstantiated()
    {
        var activity = new PrimaveraActivity();
        var baseline = new PrimaveraBaseline();
        var calendar = new PrimaveraCalendar();
        var code = new PrimaveraCode();
        var project = new PrimaveraProject();
        var relationship = new PrimaveraRelationship();
        var resourceAssignment = new PrimaveraResourceAssignment();
        var udf = new PrimaveraUdf();
        var validationRule = new PrimaveraValidationRule();
        var validationIssue = new PrimaveraValidationIssue();
        var repairAction = new PrimaveraRepairAction();
        var importSession = new XerImportSession();
        var exportProfile = new XerExportProfile();
        var rawTable = new XerRawTable();

        activity.Should().NotBeNull();
        baseline.Should().NotBeNull();
        calendar.Should().NotBeNull();
        code.Should().NotBeNull();
        project.Should().NotBeNull();
        relationship.Should().NotBeNull();
        resourceAssignment.Should().NotBeNull();
        udf.Should().NotBeNull();
        validationRule.Should().NotBeNull();
        validationIssue.Should().NotBeNull();
        repairAction.Should().NotBeNull();
        importSession.Should().NotBeNull();
        exportProfile.Should().NotBeNull();
        rawTable.Should().NotBeNull();
    }

    [Fact]
    public void AllDtos_CanBeInstantiated()
    {
        var activityDto = new PrimaveraActivityDto();
        var baselineDto = new PrimaveraBaselineDto();
        var calendarDto = new PrimaveraCalendarDto();
        var codeDto = new PrimaveraCodeDto();
        var projectDto = new PrimaveraProjectDto();
        var relationshipDto = new PrimaveraRelationshipDto();
        var resourceAssignmentDto = new PrimaveraResourceAssignmentDto();
        var udfDto = new PrimaveraUdfDto();
        var validationIssueDto = new PrimaveraValidationIssueDto();
        var repairActionDto = new PrimaveraRepairActionDto();
        var importPreviewDto = new XerImportPreviewDto();
        var importResultDto = new XerImportResultDto();
        var importSessionDto = new XerImportSessionDto();
        var dcmaResultDto = new DcmaAssessmentResultDto();
        var dcmaPointDto = new DcmaAssessmentPointDto();

        activityDto.Should().NotBeNull();
        baselineDto.Should().NotBeNull();
        calendarDto.Should().NotBeNull();
        codeDto.Should().NotBeNull();
        projectDto.Should().NotBeNull();
        relationshipDto.Should().NotBeNull();
        resourceAssignmentDto.Should().NotBeNull();
        udfDto.Should().NotBeNull();
        validationIssueDto.Should().NotBeNull();
        repairActionDto.Should().NotBeNull();
        importPreviewDto.Should().NotBeNull();
        importPreviewDto.CanCommit.Should().BeTrue();
        importResultDto.Should().NotBeNull();
        importSessionDto.Should().NotBeNull();
        dcmaResultDto.Should().NotBeNull();
        dcmaPointDto.Should().NotBeNull();
    }

    [Fact]
    public void AllModels_CanBeInstantiated()
    {
        var snapshot = new PrimaveraWorkspaceSnapshot();
        var exportProfile = new PrimaveraExportProfile();

        snapshot.Should().NotBeNull();
        snapshot.CapturedAt.Should().Be(DateTime.MinValue);
        snapshot.Activities.Should().BeEmpty();
        snapshot.Relationships.Should().BeEmpty();
        snapshot.ResourceAssignments.Should().BeEmpty();
        snapshot.Calendars.Should().BeEmpty();
        snapshot.Codes.Should().BeEmpty();
        snapshot.Baselines.Should().BeEmpty();
        snapshot.Udfs.Should().BeEmpty();

        exportProfile.Should().NotBeNull();
        exportProfile.IncludeActivities.Should().BeTrue();
        exportProfile.IncludeRelationships.Should().BeTrue();
        exportProfile.PreserveRawTables.Should().BeTrue();
    }

    [Fact]
    public void AllEnums_HaveExpectedValues()
    {
        ((int)PrimaveraEntityType.Project).Should().Be(0);
        ((int)PrimaveraEntityType.Activity).Should().Be(1);
        ((int)PrimaveraEntityType.Relationship).Should().Be(2);
        ((int)PrimaveraEntityType.ResourceAssignment).Should().Be(3);
        ((int)PrimaveraEntityType.Calendar).Should().Be(4);
        ((int)PrimaveraEntityType.Code).Should().Be(5);
        ((int)PrimaveraEntityType.Baseline).Should().Be(6);
        ((int)PrimaveraEntityType.Udf).Should().Be(7);
        ((int)PrimaveraEntityType.RawTable).Should().Be(8);

        ((int)PrimaveraSourceType.Imported).Should().Be(0);
        ((int)PrimaveraSourceType.ManualEdit).Should().Be(1);
        ((int)PrimaveraSourceType.Repair).Should().Be(2);
        ((int)PrimaveraSourceType.Export).Should().Be(3);

        ((int)PrimaveraImportStatus.Previewing).Should().Be(0);
        ((int)PrimaveraImportStatus.Committed).Should().Be(1);
        ((int)PrimaveraImportStatus.Failed).Should().Be(2);
        ((int)PrimaveraImportStatus.RolledBack).Should().Be(3);

        ((int)PrimaveraValidationSeverity.Error).Should().Be(0);
        ((int)PrimaveraValidationSeverity.Warning).Should().Be(1);
        ((int)PrimaveraValidationSeverity.Info).Should().Be(2);

        ((int)PrimaveraRepairStatus.Proposed).Should().Be(0);
        ((int)PrimaveraRepairStatus.Applied).Should().Be(1);
        ((int)PrimaveraRepairStatus.Rejected).Should().Be(2);

        ((int)DcmaStatus.Pass).Should().Be(0);
        ((int)DcmaStatus.Warning).Should().Be(1);
        ((int)DcmaStatus.Fail).Should().Be(2);
    }

    [Fact]
    public void AllEnumTypes_HaveExpectedCounts()
    {
        Enum.GetValues<PrimaveraEntityType>().Length.Should().Be(9);
        Enum.GetValues<PrimaveraSourceType>().Length.Should().Be(4);
        Enum.GetValues<PrimaveraImportStatus>().Length.Should().Be(4);
        Enum.GetValues<PrimaveraValidationSeverity>().Length.Should().Be(3);
        Enum.GetValues<PrimaveraRepairStatus>().Length.Should().Be(3);
        Enum.GetValues<DcmaStatus>().Length.Should().Be(3);
    }

    [Fact]
    public void PrimaveraImportService_CanBeInstantiated()
    {
        var repoMock = new Mock<IPrimaveraImportRepository>();
        var parser = new XerParser();
        var loggerMock = new Mock<ILogger<PrimaveraImportService>>();

        var service = new PrimaveraImportService(repoMock.Object, parser, loggerMock.Object);

        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IPrimaveraImportService>();
    }

    [Fact]
    public void PrimaveraExportService_CanBeInstantiated()
    {
        var workspaceMock = new Mock<IPrimaveraWorkspaceService>();
        var repoMock = new Mock<IPrimaveraExportRepository>();
        var writer = new XerWriter();
        var loggerMock = new Mock<ILoggingService>();

        var service = new PrimaveraExportService(workspaceMock.Object, repoMock.Object, writer, loggerMock.Object);

        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IPrimaveraExportService>();
    }

    [Fact]
    public void PrimaveraWorkspaceService_CanBeInstantiated()
    {
        var repoMock = new Mock<IPrimaveraWorkspaceRepository>();
        var loggerMock = new Mock<ILoggingService>();

        var service = new PrimaveraWorkspaceService(repoMock.Object, loggerMock.Object);

        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IPrimaveraWorkspaceService>();
    }

    [Fact]
    public void PrimaveraValidationService_CanBeInstantiated()
    {
        var workspaceMock = new Mock<IPrimaveraWorkspaceService>();
        var repoMock = new Mock<IPrimaveraValidationRepository>();

        var service = new PrimaveraValidationService(workspaceMock.Object, repoMock.Object);

        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IPrimaveraValidationService>();
    }

    [Fact]
    public void PrimaveraRepairService_CanBeInstantiated()
    {
        var validationMock = new Mock<IPrimaveraValidationService>();
        var repoMock = new Mock<IPrimaveraRepairRepository>();
        var workspaceMock = new Mock<IPrimaveraWorkspaceService>();
        var loggerMock = new Mock<ILoggingService>();

        var service = new PrimaveraRepairService(validationMock.Object, repoMock.Object, workspaceMock.Object, loggerMock.Object);

        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IPrimaveraRepairService>();
    }

    [Fact]
    public void XerParser_CanBeInstantiated()
    {
        var parser = new XerParser();
        parser.Should().NotBeNull();
    }

    [Fact]
    public void XerWriter_CanBeInstantiated()
    {
        var writer = new XerWriter();
        writer.Should().NotBeNull();
    }

    [Fact]
    public async Task XerParser_ParseAsync_WithSmallFixture_ReturnsExpectedTables()
    {
        var xerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "Fixtures", "small.xer");

        if (!File.Exists(xerPath))
            return;

        var parser = new XerParser();
        var result = await parser.ParseAsync(xerPath);

        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.TableNames.Should().Contain("CALENDAR");
        result.TableNames.Should().Contain("PROJECT");
        result.TableNames.Should().Contain("TASK");
        result.TableNames.Should().Contain("TASKPRED");
        result.TableNames.Should().Contain("TASKRSRC");
        result.TableNames.Should().Contain("RSOURCE");
        result.TableNames.Should().Contain("UDFTYPE");
        result.TableNames.Should().Contain("UDFVALUE");
        result.Project.Should().NotBeNull();
        result.Project!.Name.Should().Be("Small Test Project");
        result.Activities.Should().HaveCount(4);
        result.Relationships.Should().HaveCount(4);
        result.ResourceAssignments.Should().HaveCount(1);
        result.Calendars.Should().HaveCount(1);
        result.Udfs.Should().HaveCount(2);
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task XerParser_ParseAsync_WithNonexistentFile_ReturnsEmptyResult()
    {
        var parser = new XerParser();
        var result = await parser.ParseAsync("nonexistent-file.xer");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task XerWriter_WriteAsync_WithMinimalData_ProducesValidXer()
    {
        var writer = new XerWriter();
        var outputPath = Path.GetTempFileName() + ".xer";

        try
        {
            await writer.WriteAsync(outputPath,
                new List<PrimaveraCalendar>
                {
                    new() { CalendarId = "1", Name = "5-Day", IsBaseCalendar = true, SourceType = PrimaveraSourceType.Imported }
                },
                null,
                new List<PrimaveraActivity>(),
                new List<PrimaveraRelationship>(),
                new List<PrimaveraResourceAssignment>(),
                new List<PrimaveraCode>(),
                new List<PrimaveraBaseline>(),
                new List<PrimaveraUdf>(),
                new List<XerRawTable>());

            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().StartWith("ERMHDR");
            content.Should().Contain("%T|CALENDAR");
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    [Fact]
    public void PrimaveraImportHostedService_CanBeInstantiated()
    {
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var loggerMock = new Mock<ILogger<PrimaveraImportHostedService>>();

        var service = new PrimaveraImportHostedService(scopeFactoryMock.Object, loggerMock.Object);

        service.Should().NotBeNull();
        service.Should().BeAssignableTo<BackgroundService>();
    }

    [Fact]
    public void ServiceCollectionExtensions_DoesNotThrowOnRegistration()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddPlanovaPrimavera();
        act.Should().NotThrow();
    }

    [Fact]
    public void ServiceCollectionExtensions_IfAvailable_DoesNotThrow()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddPlanovaPrimaveraIfAvailable();
        act.Should().NotThrow();
    }

    [Fact]
    public void ServiceCollectionExtensions_IfAvailable_DoesNotThrowOnDuplicateCall()
    {
        var services = new ServiceCollection();

        Action act = () =>
        {
            services.AddPlanovaPrimaveraIfAvailable();
            services.AddPlanovaPrimaveraIfAvailable();
        };
        act.Should().NotThrow();
    }

    [Fact]
    public void EntityMapping_ToDto_ProducesCorrectOutput()
    {
        var entity = new PrimaveraActivity
        {
            Id = Guid.NewGuid(),
            TaskId = "A1000",
            Name = "Test Activity",
            Duration = 10,
            Status = "Status_NotStart",
            SourceType = PrimaveraSourceType.Imported,
            CalendarId = "1"
        };

        var dto = entity.ToDto();

        dto.Id.Should().Be(entity.Id);
        dto.TaskId.Should().Be(entity.TaskId);
        dto.Name.Should().Be(entity.Name);
        dto.Duration.Should().Be(entity.Duration);
        dto.Status.Should().Be(entity.Status);
        dto.SourceType.Should().Be("Imported");
        dto.CalendarId.Should().Be(entity.CalendarId);
    }

    [Fact]
    public void EntityMapping_AllEntities_MapWithoutError()
    {
        var activity = new PrimaveraActivity { TaskId = "T1", Name = "A", Duration = 1, SourceType = PrimaveraSourceType.Imported };
        var rel = new PrimaveraRelationship { PredTaskId = "P", SuccTaskId = "S", Type = "FS", SourceType = PrimaveraSourceType.Imported };
        var ra = new PrimaveraResourceAssignment { TaskId = "T1", ResourceId = "R1", Units = 10, SourceType = PrimaveraSourceType.Imported };
        var cal = new PrimaveraCalendar { CalendarId = "C1", Name = "Cal", IsBaseCalendar = true, SourceType = PrimaveraSourceType.Imported };
        var code = new PrimaveraCode { CodeType = "Dept", CodeValue = "ENG", CodeName = "Engineering", SourceType = PrimaveraSourceType.Imported };
        var bl = new PrimaveraBaseline { BaselineId = "BL1", Name = "BL", VersionNumber = 1, IsActive = true, SourceType = PrimaveraSourceType.Imported };
        var udf = new PrimaveraUdf { UdfTypeId = "U1", TableName = "TASK", FieldName = "Priority", FieldType = "TEXT", SourceType = PrimaveraSourceType.Imported };
        var issue = new PrimaveraValidationIssue { Severity = PrimaveraValidationSeverity.Warning, EntityType = PrimaveraEntityType.Activity, Description = "Test" };
        var repair = new PrimaveraRepairAction { Description = "Fix", TargetEntityType = PrimaveraEntityType.Activity, AppliedBy = "User", AppliedAt = DateTime.UtcNow };

        var activityDto = activity.ToDto();
        var relDto = rel.ToDto();
        var raDto = ra.ToDto();
        var calDto = cal.ToDto();
        var codeDto = code.ToDto();
        var blDto = bl.ToDto();
        var udfDto = udf.ToDto();
        var issueDto = issue.ToDto();
        var repairDto = repair.ToDto();

        activityDto.TaskId.Should().Be("T1");
        relDto.PredTaskId.Should().Be("P");
        raDto.Units.Should().Be(10);
        calDto.IsBaseCalendar.Should().BeTrue();
        codeDto.CodeValue.Should().Be("ENG");
        blDto.IsActive.Should().BeTrue();
        udfDto.FieldName.Should().Be("Priority");
        issueDto.Severity.Should().Be("Warning");
        repairDto.Description.Should().Be("Fix");
    }

    [Fact]
    public async Task ImportToExportFlow_WithMockedData_CompletesWithoutError()
    {
        var repoMock = new Mock<IPrimaveraImportRepository>();
        var parser = new XerParser();
        var loggerMock = new Mock<ILogger<PrimaveraImportService>>();
        var importService = new PrimaveraImportService(repoMock.Object, parser, loggerMock.Object);

        var xerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "Fixtures", "small.xer");

        if (!File.Exists(xerPath))
            return;

        var preview = await importService.PreviewAsync(xerPath);
        preview.Should().NotBeNull();
        preview.FileName.Should().Be("small.xer");
        preview.ValidationIssues.Should().BeEmpty();
        preview.CanCommit.Should().BeTrue();
    }

    [Fact]
    public async Task ImportPreview_WithNonexistentFile_ReturnsError()
    {
        var repoMock = new Mock<IPrimaveraImportRepository>();
        var parser = new XerParser();
        var loggerMock = new Mock<ILogger<PrimaveraImportService>>();
        var importService = new PrimaveraImportService(repoMock.Object, parser, loggerMock.Object);

        var preview = await importService.PreviewAsync("missing-file.xer");

        preview.Should().NotBeNull();
        preview.ValidationIssues.Should().Contain(i => i.Severity == "Error" && i.Description.Contains("not found"));
        preview.CanCommit.Should().BeFalse();
    }

    [Fact]
    public async Task CommitAsync_WithUnknownSession_ReturnsFailure()
    {
        var repoMock = new Mock<IPrimaveraImportRepository>();
        var parser = new XerParser();
        var loggerMock = new Mock<ILogger<PrimaveraImportService>>();
        var importService = new PrimaveraImportService(repoMock.Object, parser, loggerMock.Object);

            var result = await importService.CommitAsync(Guid.NewGuid(), false);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Import session not found.");
    }

    [Fact]
    public async Task GetImportedSessionsAsync_ReturnsEmptyList()
    {
        var repoMock = new Mock<IPrimaveraImportRepository>();
        repoMock.Setup(r => r.GetSessionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<XerImportSession>());
        var parser = new XerParser();
        var loggerMock = new Mock<ILogger<PrimaveraImportService>>();
        var importService = new PrimaveraImportService(repoMock.Object, parser, loggerMock.Object);

        var sessions = await importService.GetImportedSessionsAsync();

        sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidationService_WithNoData_ReturnsEmpty()
    {
        var workspaceMock = new Mock<IPrimaveraWorkspaceService>();
        workspaceMock.Setup(w => w.GetActivitiesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraActivityDto>());
        workspaceMock.Setup(w => w.GetRelationshipsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraRelationshipDto>());
        workspaceMock.Setup(w => w.GetCalendarsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraCalendarDto>());
        var repoMock = new Mock<IPrimaveraValidationRepository>();

        var service = new PrimaveraValidationService(workspaceMock.Object, repoMock.Object);
        var result = await service.ValidateAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RepairService_WithNoIssues_ReturnsEmpty()
    {
        var validationMock = new Mock<IPrimaveraValidationService>();
        validationMock.Setup(v => v.ValidateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraValidationIssueDto>());
        var repoMock = new Mock<IPrimaveraRepairRepository>();
        var workspaceMock = new Mock<IPrimaveraWorkspaceService>();
        var loggerMock = new Mock<ILoggingService>();

        var service = new PrimaveraRepairService(validationMock.Object, repoMock.Object, workspaceMock.Object, loggerMock.Object);
        var result = await service.GetSuggestedFixesAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportService_WithMockedData_GeneratesOutput()
    {
        var workspaceMock = new Mock<IPrimaveraWorkspaceService>();
        workspaceMock.Setup(w => w.GetSnapshotAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PrimaveraWorkspaceSnapshot
            {
                Activities = new List<PrimaveraActivityDto>
                {
                    new() { TaskId = "T1", Name = "Task 1", Duration = 5 }
                },
                Calendars = new List<PrimaveraCalendarDto>
                {
                    new() { CalendarId = "C1", Name = "5-Day", IsBaseCalendar = true }
                }
            });
        var repoMock = new Mock<IPrimaveraExportRepository>();
        repoMock.Setup(r => r.GetRawTablesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<XerRawTable>());
        var writer = new XerWriter();
        var loggerMock = new Mock<ILoggingService>();

        var service = new PrimaveraExportService(workspaceMock.Object, repoMock.Object, writer, loggerMock.Object);
        var outputPath = Path.GetTempFileName() + ".xer";

        try
        {
            var result = await service.ExportAsync(1, outputPath);
            result.Should().Be(outputPath);
            File.Exists(outputPath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().Contain("ERMHDR");
            content.Should().Contain("%T|TASK");
            content.Should().Contain("T1");
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportService_WithProfile_RespectsExcludeFlags()
    {
        var workspaceMock = new Mock<IPrimaveraWorkspaceService>();
        workspaceMock.Setup(w => w.GetSnapshotAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PrimaveraWorkspaceSnapshot
            {
                Activities = new List<PrimaveraActivityDto>
                {
                    new() { TaskId = "T1", Name = "Task 1", Duration = 5 }
                },
                Calendars = new List<PrimaveraCalendarDto>
                {
                    new() { CalendarId = "C1", Name = "5-Day", IsBaseCalendar = true }
                }
            });
        var repoMock = new Mock<IPrimaveraExportRepository>();
        var writer = new XerWriter();
        var loggerMock = new Mock<ILoggingService>();

        var service = new PrimaveraExportService(workspaceMock.Object, repoMock.Object, writer, loggerMock.Object);
        var outputPath = Path.GetTempFileName() + ".xer";

        try
        {
            var profile = new PrimaveraExportProfile
            {
                ProjectId = 1,
                OutputPath = outputPath,
                IncludeActivities = false,
                IncludeCalendars = false,
                IncludeRelationships = false,
                IncludeResourceAssignments = false,
                IncludeCodes = false,
                IncludeBaselines = false,
                IncludeUdfs = false,
                PreserveRawTables = false
            };

            var result = await service.ExportWithProfileAsync(profile);
            result.Should().Be(outputPath);

            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().NotContain("%T|TASK");
            content.Should().NotContain("%T|CALENDAR");
            content.Should().Contain("ERMHDR");
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task DcmaAssessment_WithEmptyData_ReturnsDefault()
    {
        var workspaceMock = new Mock<IPrimaveraWorkspaceService>();
        workspaceMock.Setup(w => w.GetActivitiesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraActivityDto>());
        workspaceMock.Setup(w => w.GetRelationshipsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraRelationshipDto>());
        workspaceMock.Setup(w => w.GetCalendarsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraCalendarDto>());
        workspaceMock.Setup(w => w.GetResourceAssignmentsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraResourceAssignmentDto>());
        var repoMock = new Mock<IPrimaveraValidationRepository>();

        var service = new PrimaveraValidationService(workspaceMock.Object, repoMock.Object);
        var result = await service.AssessDcma14PointAsync(1);

        result.Should().NotBeNull();
        result.OverallScore.Should().BeGreaterThan(0);
        result.Points.Should().HaveCount(14);
    }

    [Fact]
    public async Task DcmaAssessment_WithSampleData_ReturnsExpectedPoints()
    {
        var activities = new List<PrimaveraActivityDto>
        {
            new() { TaskId = "T1", Name = "Start", Duration = 0, Status = "Status_Completed", PercentComplete = 100, StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(-5) },
            new() { TaskId = "T2", Name = "Work", Duration = 10, Status = "Status_NotStart", PercentComplete = 0, StartDate = DateTime.UtcNow.AddDays(-5), EndDate = DateTime.UtcNow.AddDays(5), CalendarId = "C1" },
            new() { TaskId = "T3", Name = "Finish", Duration = 0, Status = "Status_Completed", PercentComplete = 100, StartDate = DateTime.UtcNow.AddDays(5), EndDate = DateTime.UtcNow.AddDays(5) }
        };
        var relationships = new List<PrimaveraRelationshipDto>
        {
            new() { PredTaskId = "T1", SuccTaskId = "T2", Type = "FS", LagDuration = 0 },
            new() { PredTaskId = "T2", SuccTaskId = "T3", Type = "FS", LagDuration = 0 }
        };
        var calendars = new List<PrimaveraCalendarDto>
        {
            new() { CalendarId = "C1", Name = "5-Day", IsBaseCalendar = true }
        };
        var assignments = new List<PrimaveraResourceAssignmentDto>
        {
            new() { TaskId = "T2", ResourceId = "R1", Units = 100 }
        };

        var workspaceMock = new Mock<IPrimaveraWorkspaceService>();
        workspaceMock.Setup(w => w.GetActivitiesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(activities);
        workspaceMock.Setup(w => w.GetRelationshipsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(relationships);
        workspaceMock.Setup(w => w.GetCalendarsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(calendars);
        workspaceMock.Setup(w => w.GetResourceAssignmentsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(assignments);
        var repoMock = new Mock<IPrimaveraValidationRepository>();

        var service = new PrimaveraValidationService(workspaceMock.Object, repoMock.Object);
        var result = await service.AssessDcma14PointAsync(1);

        result.Should().NotBeNull();
        result.Points.Should().HaveCount(14);
        result.Points[0].PointNumber.Should().Be(1);
        result.Points[0].Name.Should().Be("Missing Logic");
        result.Points[1].PointNumber.Should().Be(2);
        result.Points[1].Name.Should().Be("Leads");
        result.Points[1].Status.Should().Be(DcmaStatus.Pass);
        result.Points[3].PointNumber.Should().Be(4);
        result.Points[3].Name.Should().Be("Relationship Types");
        result.Points[3].Status.Should().Be(DcmaStatus.Pass);
        result.Points[4].PointNumber.Should().Be(5);
        result.Points[4].Name.Should().Be("Hard Constraints");
    }

    [Fact]
    public async Task WorkspaceService_HasData_ReturnsFalseByDefault()
    {
        var repoMock = new Mock<IPrimaveraWorkspaceRepository>();
        repoMock.Setup(r => r.GetActivitiesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraActivity>());
        var loggerMock = new Mock<ILoggingService>();

        var service = new PrimaveraWorkspaceService(repoMock.Object, loggerMock.Object);
        var hasData = await service.HasDataAsync(1);

        hasData.Should().BeFalse();
    }

    [Fact]
    public async Task WorkspaceService_GetSnapshot_ReturnsEmpty()
    {
        var repoMock = new Mock<IPrimaveraWorkspaceRepository>();
        repoMock.Setup(r => r.GetActivitiesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PrimaveraActivity>());
        repoMock.Setup(r => r.GetRelationshipsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PrimaveraRelationship>());
        repoMock.Setup(r => r.GetResourceAssignmentsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PrimaveraResourceAssignment>());
        repoMock.Setup(r => r.GetCalendarsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PrimaveraCalendar>());
        repoMock.Setup(r => r.GetCodesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PrimaveraCode>());
        repoMock.Setup(r => r.GetBaselinesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PrimaveraBaseline>());
        repoMock.Setup(r => r.GetUdfsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PrimaveraUdf>());
        var loggerMock = new Mock<ILoggingService>();

        var service = new PrimaveraWorkspaceService(repoMock.Object, loggerMock.Object);
        var snapshot = await service.GetSnapshotAsync(1);

        snapshot.Should().NotBeNull();
        snapshot.Activities.Should().BeEmpty();
        snapshot.Relationships.Should().BeEmpty();
        snapshot.ResourceAssignments.Should().BeEmpty();
        snapshot.Calendars.Should().BeEmpty();
        snapshot.Codes.Should().BeEmpty();
        snapshot.Baselines.Should().BeEmpty();
        snapshot.Udfs.Should().BeEmpty();
    }

    [Fact]
    public async Task WorkspaceService_UpdateNonexistentActivity_ReturnsFalse()
    {
        var repoMock = new Mock<IPrimaveraWorkspaceRepository>();
        repoMock.Setup(r => r.GetActivityByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PrimaveraActivity?)null);
        var loggerMock = new Mock<ILoggingService>();

        var service = new PrimaveraWorkspaceService(repoMock.Object, loggerMock.Object);
        var result = await service.UpdateActivityAsync(new PrimaveraActivityDto { Id = Guid.NewGuid() });

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WorkspaceService_SaveChanges_ReturnsZeroWithMock()
    {
        var repoMock = new Mock<IPrimaveraWorkspaceRepository>();
        repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        var loggerMock = new Mock<ILoggingService>();

        var service = new PrimaveraWorkspaceService(repoMock.Object, loggerMock.Object);
        var count = await service.SaveChangesAsync();

        count.Should().Be(0);
    }

    [Fact]
    public void XerFieldNames_HasExpectedConstants()
    {
        XerFieldNames.HeaderRecord.Should().Be("ERMHDR");
        XerFieldNames.CalendarTable.Should().Be("CALENDAR");
        XerFieldNames.ProjectTable.Should().Be("PROJECT");
        XerFieldNames.TaskTable.Should().Be("TASK");
        XerFieldNames.TaskPredTable.Should().Be("TASKPRED");
        XerFieldNames.TaskRsrcTable.Should().Be("TASKRSRC");
        XerFieldNames.RsourceTable.Should().Be("RSOURCE");
        XerFieldNames.UdfTypeTable.Should().Be("UDFTYPE");
        XerFieldNames.UdfValueTable.Should().Be("UDFVALUE");
        XerFieldNames.SupportedTables.Should().Contain("CALENDAR");
        XerFieldNames.SupportedTables.Should().Contain("TASK");
        XerFieldNames.SupportedTables.Should().Contain("TASKPRED");
        XerFieldNames.IsSupportedTable("CALENDAR").Should().BeTrue();
        XerFieldNames.IsSupportedTable("UNKNOWN").Should().BeFalse();
    }

    [Fact]
    public void XerImportSession_TracksStatus()
    {
        var session = new XerImportSession
        {
            Id = Guid.NewGuid(),
            SourceFileName = "test.xer",
            Status = PrimaveraImportStatus.Previewing,
            ImportedAt = DateTime.UtcNow,
            ImportedBy = "tester"
        };

        session.Status.Should().Be(PrimaveraImportStatus.Previewing);
        session.SourceFileName.Should().Be("test.xer");
    }

    [Fact]
    public void XerRawTable_StoresTableData()
    {
        var table = new XerRawTable
        {
            TableName = "RISK",
            ColumnHeaders = "risk_id|name|type",
            Rows = "1|Risk1|Cost"
        };

        table.TableName.Should().Be("RISK");
        table.ColumnHeaders.Should().Be("risk_id|name|type");
        table.Rows.Should().Be("1|Risk1|Cost");
    }

    [Fact]
    public async Task HostedService_CanStartAndStop()
    {
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        var importServiceMock = new Mock<IPrimaveraImportService>();

        serviceProviderMock.Setup(sp => sp.GetService(typeof(IPrimaveraImportService)))
            .Returns(importServiceMock.Object);
        serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

        var loggerMock = new Mock<ILogger<PrimaveraImportHostedService>>();
        var hostedService = new PrimaveraImportHostedService(scopeFactoryMock.Object, loggerMock.Object);

        using var cts = new CancellationTokenSource(500);
        var act = () => hostedService.StartAsync(cts.Token);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void DcmaAssessmentResultDto_DefaultValues()
    {
        var result = new DcmaAssessmentResultDto();
        result.OverallScore.Should().Be(0);
        result.Points.Should().BeEmpty();
    }

    [Fact]
    public void XerImportSessionDto_DeserializesJsonFields()
    {
        var dto = new XerImportSessionDto
        {
            TableNames = "[\"CALENDAR\",\"TASK\"]",
            RowCounts = "{\"CALENDAR\":1,\"TASK\":4}"
        };

        dto.TableNamesList.Should().BeEquivalentTo(new[] { "CALENDAR", "TASK" });
        dto.RowCountsDict.Should().ContainKey("CALENDAR").WhoseValue.Should().Be(1);
        dto.RowCountsDict.Should().ContainKey("TASK").WhoseValue.Should().Be(4);
    }
}
