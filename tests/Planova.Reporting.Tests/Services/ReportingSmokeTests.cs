using FluentAssertions;
using NSubstitute;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Application.Services;
using Planova.Reporting.Background;
using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;
using ReportingReportType = Planova.Reporting.Domain.Enums.ReportType;
using Planova.Reporting.Extensions;
using Planova.Shared.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Planova.Reporting.Tests.Services;

public class ReportingSmokeTests
{
    [Fact]
    public void AllEnums_HaveExpectedValues()
    {
        ((int)ReportingReportType.Daily).Should().Be(0);
        ((int)ReportingReportType.Weekly).Should().Be(1);
        ((int)ReportingReportType.Monthly).Should().Be(2);
        ((int)ReportingReportType.Executive).Should().Be(3);

        ((int)ReportStatus.Draft).Should().Be(0);
        ((int)ReportStatus.Final).Should().Be(1);
        ((int)ReportStatus.Archived).Should().Be(2);

        ((int)ScheduleFrequency.Daily).Should().Be(0);
        ((int)ScheduleFrequency.Weekly).Should().Be(1);
        ((int)ScheduleFrequency.Monthly).Should().Be(2);

        ((int)ExportFormat.Excel).Should().Be(0);
        ((int)ExportFormat.Pdf).Should().Be(1);
        ((int)ExportFormat.Word).Should().Be(2);

        ((int)PartyRole.Client).Should().Be(0);
        ((int)PartyRole.MainContractor).Should().Be(1);
        ((int)PartyRole.SubContractor).Should().Be(2);

        ((int)ReportSectionType.Text).Should().Be(0);
        ((int)ReportSectionType.Table).Should().Be(1);
        ((int)ReportSectionType.Chart).Should().Be(2);
        ((int)ReportSectionType.Image).Should().Be(3);
        ((int)ReportSectionType.AiNarrative).Should().Be(4);
    }

    [Fact]
    public void ReportInstance_CanBeCreated()
    {
        var instance = new ReportInstance
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            ReportType = ReportingReportType.Daily,
            Title = "Test Report",
            Status = ReportStatus.Draft,
            PeriodStart = DateTime.Today,
            PeriodEnd = DateTime.Today,
            GeneratedAt = DateTime.UtcNow,
            DataSnapshotJson = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        instance.Should().NotBeNull();
        instance.Id.Should().NotBeEmpty();
        instance.ReportType.Should().Be(ReportingReportType.Daily);
        instance.Status.Should().Be(ReportStatus.Draft);
    }

    [Fact]
    public void ReportTemplate_CanBeCreated()
    {
        var template = new ReportTemplate
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            ReportType = ReportingReportType.Weekly,
            Name = "Default Weekly",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };

        template.Should().NotBeNull();
        template.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void ReportSchedule_CanBeCreated()
    {
        var schedule = new ReportSchedule
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            ReportType = ReportingReportType.Monthly,
            Frequency = ScheduleFrequency.Monthly,
            TimeOfDay = TimeSpan.FromHours(8),
            TimeZoneId = "UTC",
            ExportFormats = "Excel,PDF",
            IsActive = true,
            MaxRetries = 3,
            CreatedAt = DateTime.UtcNow
        };

        schedule.Should().NotBeNull();
        schedule.IsActive.Should().BeTrue();
        schedule.Frequency.Should().Be(ScheduleFrequency.Monthly);
    }

    [Fact]
    public void ReportExport_CanBeCreated()
    {
        var export = new ReportExport
        {
            Id = Guid.NewGuid(),
            ReportInstanceId = Guid.NewGuid(),
            Format = ExportFormat.Pdf,
            FilePath = @"C:\exports\report.pdf",
            FileSizeBytes = 1024,
            ExportedAt = DateTime.UtcNow,
            ExportedBy = "test-user"
        };

        export.Should().NotBeNull();
        export.Format.Should().Be(ExportFormat.Pdf);
        export.FileSizeBytes.Should().Be(1024);
    }

    [Fact]
    public void ProjectParty_CanBeCreated()
    {
        var party = new ProjectParty
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            Name = "Test Client",
            Role = PartyRole.Client,
            LogoPath = null,
            CreatedAt = DateTime.UtcNow
        };

        party.Should().NotBeNull();
        party.Role.Should().Be(PartyRole.Client);
    }

    [Fact]
    public void ReportAiService_GeneratesNarrative_ForAllTypes()
    {
        var logger = Substitute.For<ILoggingService>();
        var aiService = new ReportAiService(logger);

        var dailyNarrative = aiService.GenerateNarrativeAsync(ReportingReportType.Daily, new object()).Result;
        dailyNarrative.Should().Contain("Daily");

        var weeklyNarrative = aiService.GenerateNarrativeAsync(ReportingReportType.Weekly, new object()).Result;
        weeklyNarrative.Should().Contain("Weekly");

        var monthlyNarrative = aiService.GenerateNarrativeAsync(ReportingReportType.Monthly, new object()).Result;
        monthlyNarrative.Should().Contain("Monthly");

        var execNarrative = aiService.GenerateNarrativeAsync(ReportingReportType.Executive, new object()).Result;
        execNarrative.Should().Contain("Executive");
    }

    [Fact]
    public async Task ReportEngine_GeneratesAndStoresInstance()
    {
        var instanceRepo = Substitute.For<IReportInstanceRepository>();
        var templateRepo = Substitute.For<IReportTemplateRepository>();
        var logger = Substitute.For<ILoggingService>();
        var serviceProvider = Substitute.For<IServiceProvider>();

        instanceRepo.AddAsync(Arg.Any<ReportInstance>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var engine = new ReportEngine(instanceRepo, templateRepo, serviceProvider, logger);

        var result = await engine.GenerateAsync(1, ReportingReportType.Daily, DateTime.Today, DateTime.Today);

        result.Should().NotBeNull();
        result.ReportType.Should().Be("Daily");
        result.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task ReportExportService_ExcelExport_DoesNotThrow()
    {
        var instanceRepo = Substitute.For<IReportInstanceRepository>();
        var logger = Substitute.For<ILoggingService>();
        var instanceId = Guid.NewGuid();

        instanceRepo.GetByIdAsync(instanceId, Arg.Any<CancellationToken>())
            .Returns(new ReportInstance
            {
                Id = instanceId,
                ProjectId = 1,
                ReportType = ReportingReportType.Daily,
                Title = "Smoke Test",
                Status = ReportStatus.Draft,
                PeriodStart = DateTime.Today,
                PeriodEnd = DateTime.Today,
                GeneratedAt = DateTime.UtcNow,
                DataSnapshotJson = "{}",
                Exports = [],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        var exportService = new ReportExportService(instanceRepo, logger);

        var act = () => exportService.ExportToExcelAsync(instanceId, new { });

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ReportExportService_PdfExport_ThrowsLicenseException()
    {
        var instanceRepo = Substitute.For<IReportInstanceRepository>();
        var logger = Substitute.For<ILoggingService>();
        var instanceId = Guid.NewGuid();

        instanceRepo.GetByIdAsync(instanceId, Arg.Any<CancellationToken>())
            .Returns(new ReportInstance
            {
                Id = instanceId,
                ProjectId = 1,
                ReportType = ReportingReportType.Weekly,
                Title = "PDF Smoke Test",
                Status = ReportStatus.Draft,
                PeriodStart = DateTime.Today.AddDays(-7),
                PeriodEnd = DateTime.Today,
                GeneratedAt = DateTime.UtcNow,
                DataSnapshotJson = "{}",
                Exports = [],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        var exportService = new ReportExportService(instanceRepo, logger);

        var act = () => exportService.ExportToPdfAsync(instanceId, new { });

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Thank you for choosing QuestPDF*");
    }

    [Fact]
    public async Task ReportExportService_WordExport_DoesNotThrow()
    {
        var instanceRepo = Substitute.For<IReportInstanceRepository>();
        var logger = Substitute.For<ILoggingService>();
        var instanceId = Guid.NewGuid();

        instanceRepo.GetByIdAsync(instanceId, Arg.Any<CancellationToken>())
            .Returns(new ReportInstance
            {
                Id = instanceId,
                ProjectId = 1,
                ReportType = ReportingReportType.Monthly,
                Title = "Word Smoke Test",
                Status = ReportStatus.Draft,
                PeriodStart = DateTime.Today.AddMonths(-1),
                PeriodEnd = DateTime.Today,
                GeneratedAt = DateTime.UtcNow,
                DataSnapshotJson = "{}",
                Exports = [],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        var exportService = new ReportExportService(instanceRepo, logger);

        var act = () => exportService.ExportToWordAsync(instanceId, new { });

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void ReportSchedulerService_NextRunComputation_Daily()
    {
        var repo = Substitute.For<IReportScheduleRepository>();
        var logger = Substitute.For<ILoggingService>();
        var scheduler = new ReportSchedulerService(repo, logger);

        var schedule = new ReportSchedule
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            ReportType = ReportingReportType.Daily,
            Frequency = ScheduleFrequency.Daily,
            TimeOfDay = TimeSpan.FromHours(8),
            TimeZoneId = "UTC",
            ExportFormats = "Excel",
            IsActive = true,
            MaxRetries = 3,
            CreatedAt = DateTime.UtcNow
        };

        var nextRun = scheduler.ComputeNextRunAsync(schedule).Result;

        nextRun.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void DI_Registration_AllDescriptorTypesAreCorrect()
    {
        var services = new ServiceCollection();
        services.AddPlanovaReporting();

        var engineDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IReportEngine));
        engineDescriptor.Should().NotBeNull();
        engineDescriptor!.ImplementationType.Should().Be<ReportEngine>();
        engineDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

        var aiDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IReportAiService));
        aiDescriptor.Should().NotBeNull();
        aiDescriptor!.ImplementationType.Should().Be<ReportAiService>();
        aiDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

        var exportDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IReportExportService));
        exportDescriptor.Should().NotBeNull();

        var schedulerDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IReportSchedulerService));
        schedulerDescriptor.Should().NotBeNull();

        var settingsDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IReportSettingsService));
        settingsDescriptor.Should().NotBeNull();

        var partyDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IProjectPartyService));
        partyDescriptor.Should().NotBeNull();

        services.Any(s => s.ServiceType == typeof(IHostedService)).Should().BeTrue();
    }
}
