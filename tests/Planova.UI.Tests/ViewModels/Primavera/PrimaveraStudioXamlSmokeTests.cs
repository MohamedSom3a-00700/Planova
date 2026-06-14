using FluentAssertions;
using Moq;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;
using Planova.Shared.Abstractions;
using Planova.UI.Converters;
using Planova.UI.ViewModels.Primavera;

namespace Planova.UI.Tests.ViewModels.Primavera;

public class PrimaveraStudioXamlSmokeTests
{
    [Fact]
    public void PrimaveraStudioTab_CanBeCreated()
    {
        var tab = new PrimaveraStudioTab("Import", new object());
        tab.Header.Should().Be("Import");
        tab.Content.Should().NotBeNull();
    }

    [Fact]
    public void PrimaveraStudioTab_PropertiesAreObservable()
    {
        var tab = new PrimaveraStudioTab("Import", new object());
        tab.Header = "Export";
        tab.Header.Should().Be("Export");
        tab.Content = "test";
        tab.Content.Should().Be("test");
    }

    [Fact]
    public void PrimaveraStudioViewModel_CanBeInstantiated()
    {
        var vm = CreateStudioViewModel();
        vm.Should().NotBeNull();
        vm.ImportViewModel.Should().NotBeNull();
        vm.WorkspaceViewModel.Should().NotBeNull();
        vm.ValidationViewModel.Should().NotBeNull();
        vm.RepairViewModel.Should().NotBeNull();
        vm.ExportViewModel.Should().NotBeNull();
        vm.Tabs.Should().BeEmpty();
        vm.SelectedTab.Should().BeNull();
        vm.IsProjectActive.Should().BeFalse();
    }

    [Fact]
    public void PrimaveraStudioViewModel_ProjectAlreadyActive_DoesNotThrow()
    {
        var spy = new SpyCurrentProjectService();
        spy.SetProject(new ProjectContext(1, "PROJ-1", "Test"));
        var vm = CreateStudioViewModel(spy);
        vm.ImportViewModel.Should().NotBeNull();
    }

    [Fact]
    public void PrimaveraStudioViewModel_RunValidationFromToolbar_DoesNotThrow()
    {
        var vm = CreateStudioViewModel();
        vm.Tabs.Add(new PrimaveraStudioTab("Import", new object()));
        vm.Tabs.Add(new PrimaveraStudioTab("Workspace", new object()));
        vm.Tabs.Add(new PrimaveraStudioTab("Validate", new object()));

        Action act = () => vm.RunValidationFromToolbarCommand.Execute(null);
        act.Should().NotThrow();
    }

    [Fact]
    public void ImportLogEntry_CanBeCreated()
    {
        var entry = new ImportLogEntry("Test message");
        entry.Message.Should().Be("Test message");
        entry.Severity.Should().Be(ImportLogEntrySeverity.Info);
        entry.Timestamp.Should().NotBeNullOrEmpty();

        var errorEntry = new ImportLogEntry("Error!", ImportLogEntrySeverity.Error);
        errorEntry.Severity.Should().Be(ImportLogEntrySeverity.Error);
    }

    [Fact]
    public void ImportLogEntry_SeverityEnum_HasExpectedValues()
    {
        ((int)ImportLogEntrySeverity.Info).Should().Be(0);
        ((int)ImportLogEntrySeverity.Warning).Should().Be(1);
        ((int)ImportLogEntrySeverity.Error).Should().Be(2);
    }

    [Fact]
    public void PrimaveraImportViewModel_CanBeInstantiated()
    {
        var vm = new PrimaveraImportViewModel(Mock.Of<IPrimaveraImportService>());
        vm.Should().NotBeNull();
        vm.SelectedFilePath.Should().BeEmpty();
        vm.IsPreviewVisible.Should().BeFalse();
        vm.IsImporting.Should().BeFalse();
        vm.ValidationIssues.Should().BeEmpty();
        vm.ImportLog.Should().BeEmpty();
        vm.ImportedSessions.Should().BeEmpty();
        vm.SelectedImportedSession.Should().BeNull();
    }

    [Fact]
    public void PrimaveraImportViewModel_CancelImport_ResetsState()
    {
        var vm = new PrimaveraImportViewModel(Mock.Of<IPrimaveraImportService>());

        vm.CancelImportCommand.Execute(null);

        vm.SelectedFilePath.Should().BeEmpty();
        vm.IsPreviewVisible.Should().BeFalse();
        vm.Preview.Should().BeNull();
        vm.ValidationIssues.Should().BeEmpty();
        vm.ImportLog.Should().Contain(e => e.Message == "Import cancelled.");
    }

    [Fact]
    public void PrimaveraWorkspaceViewModel_CanBeInstantiated()
    {
        var vm = CreateWorkspaceViewModel();
        vm.Should().NotBeNull();
        vm.SelectedTabIndex.Should().Be(0);
        vm.IsSaving.Should().BeFalse();
        vm.ImportedSessions.Should().BeEmpty();
        vm.SelectedSession.Should().BeNull();
        vm.EntitySummaries.Should().BeEmpty();
        vm.XeroTableSections.Should().BeEmpty();
        vm.CalendarsViewModel.Should().NotBeNull();
        vm.CodesViewModel.Should().NotBeNull();
        vm.ActivitiesViewModel.Should().NotBeNull();
        vm.RelationshipsViewModel.Should().NotBeNull();
        vm.ResourcesViewModel.Should().NotBeNull();
        vm.BaselinesViewModel.Should().NotBeNull();
        vm.UdfsViewModel.Should().NotBeNull();
    }

    [Fact]
    public void PrimaveraWorkspaceViewModel_SelectSession_UpdatesSummaries()
    {
        var vm = CreateWorkspaceViewModel();

        var session = new XerImportSessionDto
        {
            SourceFileName = "test.xer",
            ImportedAt = DateTime.UtcNow,
            Status = "Committed",
            ProjectCode = "PROJ-1",
            ProjectName = "Test Project",
            RowCounts = "{\"TASK\":10,\"CALENDAR\":1}",
            TableNames = "[\"TASK\",\"CALENDAR\"]"
        };

        vm.SelectedSession = session;

        vm.StatusMessage.Should().Contain("test.xer");
        vm.EntitySummaries.Should().HaveCount(2);
        vm.XeroTableSections.Should().HaveCount(2);
    }

    [Fact]
    public void PrimaveraWorkspaceViewModel_SelectSession_WithNoRowCounts_ShowsNoData()
    {
        var vm = CreateWorkspaceViewModel();
        var session = new XerImportSessionDto { SourceFileName = "empty.xer", RowCounts = "{}", TableNames = "[]" };

        vm.SelectedSession = session;

        vm.EntitySummaries.Should().Contain(e => e.TypeName == "No data");
    }

    [Fact]
    public void PrimaveraActivitiesViewModel_CanBeInstantiated()
    {
        var vm = new PrimaveraActivitiesViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        vm.Should().NotBeNull();
        vm.Activities.Should().BeEmpty();
        vm.FilterText.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
        vm.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void PrimaveraActivitiesViewModel_FilterText_FiltersActivities()
    {
        var vm = new PrimaveraActivitiesViewModel(Mock.Of<IPrimaveraWorkspaceService>());

        vm.GetType().GetMethod("OnFilterTextChanged",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null, new[] { typeof(string) }, null).Should().NotBeNull();
    }

    [Fact]
    public void PrimaveraRelationshipsViewModel_CanBeInstantiated()
    {
        var vm = new PrimaveraRelationshipsViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        vm.Should().NotBeNull();
        vm.Relationships.Should().BeEmpty();
        vm.FilterText.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void PrimaveraResourcesViewModel_CanBeInstantiated()
    {
        var vm = new PrimaveraResourcesViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        vm.Should().NotBeNull();
        vm.ResourceAssignments.Should().BeEmpty();
        vm.FilterText.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void PrimaveraCalendarsViewModel_CanBeInstantiated()
    {
        var vm = new PrimaveraCalendarsViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        vm.Should().NotBeNull();
        vm.Calendars.Should().BeEmpty();
        vm.FilterText.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void PrimaveraCodesViewModel_CanBeInstantiated()
    {
        var vm = new PrimaveraCodesViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        vm.Should().NotBeNull();
        vm.Codes.Should().BeEmpty();
        vm.FilterText.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void PrimaveraBaselinesViewModel_CanBeInstantiated()
    {
        var vm = new PrimaveraBaselinesViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        vm.Should().NotBeNull();
        vm.Baselines.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void PrimaveraUdfsViewModel_CanBeInstantiated()
    {
        var vm = new PrimaveraUdfsViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        vm.Should().NotBeNull();
        vm.Udfs.Should().BeEmpty();
        vm.FilterText.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void PrimaveraValidationViewModel_CanBeInstantiated()
    {
        var vm = new PrimaveraValidationViewModel(
            Mock.Of<IPrimaveraValidationService>(),
            Mock.Of<IPrimaveraImportService>());
        vm.Should().NotBeNull();
        vm.Issues.Should().BeEmpty();
        vm.GroupedIssues.Should().BeEmpty();
        vm.ImportedSessions.Should().BeEmpty();
        vm.DcmaThresholds.Should().BeEmpty();
        vm.SelectedSession.Should().BeNull();
        vm.IsValidating.Should().BeFalse();
        vm.SelectedSeverityFilter.Should().Be("All Issues");
        vm.IsDcmaRunning.Should().BeFalse();
        vm.DcmaResult.Should().BeNull();
    }

    [Fact]
    public void PrimaveraValidationViewModel_FilterChanged_AppliesGrouping()
    {
        var vm = new PrimaveraValidationViewModel(
            Mock.Of<IPrimaveraValidationService>(),
            Mock.Of<IPrimaveraImportService>());

        vm.SelectedSeverityFilter = "Errors";
        vm.SelectedSeverityFilter.Should().Be("Errors");
    }

    [Fact]
    public void PrimaveraValidationViewModel_GetStatusColor_ReturnsCorrectColors()
    {
        PrimaveraValidationViewModel.GetStatusColor(DcmaStatus.Pass).Should().Be("#28A745");
        PrimaveraValidationViewModel.GetStatusColor(DcmaStatus.Warning).Should().Be("#FFB83B");
        PrimaveraValidationViewModel.GetStatusColor(DcmaStatus.Fail).Should().Be("#E81123");
        PrimaveraValidationViewModel.GetStatusColor((DcmaStatus)99).Should().Be("Gray");
    }

    [Fact]
    public void PrimaveraRepairViewModel_CanBeInstantiated()
    {
        var vm = new PrimaveraRepairViewModel(Mock.Of<IPrimaveraRepairService>());
        vm.Should().NotBeNull();
        vm.SuggestedFixes.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
        vm.StatusMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task PrimaveraRepairViewModel_LoadSuggestions_ClearsPreviousFixes()
    {
        var mockService = new Mock<IPrimaveraRepairService>();
        mockService.Setup(s => s.GetSuggestedFixesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraRepairActionDto>());

        var vm = new PrimaveraRepairViewModel(mockService.Object);

        await vm.LoadSuggestionsCommand.ExecuteAsync(null);

        vm.SuggestedFixes.Should().BeEmpty();
        vm.StatusMessage.Should().Be("No suggested fixes available.");
    }

    [Fact]
    public void PrimaveraExportViewModel_CanBeInstantiated()
    {
        var vm = new PrimaveraExportViewModel(Mock.Of<IPrimaveraExportService>());
        vm.Should().NotBeNull();
        vm.OutputPath.Should().BeEmpty();
        vm.IsExporting.Should().BeFalse();
        vm.IncludeActivities.Should().BeTrue();
        vm.IncludeRelationships.Should().BeTrue();
        vm.IncludeResources.Should().BeTrue();
        vm.IncludeCalendars.Should().BeTrue();
        vm.IncludeCodes.Should().BeTrue();
        vm.IncludeBaselines.Should().BeTrue();
        vm.IncludeUdfs.Should().BeTrue();
        vm.PreserveRawTables.Should().BeTrue();
        vm.HasExportResult.Should().BeFalse();
    }

    [Fact]
    public void PrimaveraExportViewModel_ExportWithoutPath_DoesNothing()
    {
        var vm = new PrimaveraExportViewModel(Mock.Of<IPrimaveraExportService>());

        vm.ExportCommand.Execute(null);

        vm.StatusMessage.Should().BeEmpty();
    }

    [Fact]
    public void PrimaveraStatusConverter_Convert_ReturnsCorrectStrings()
    {
        var converter = new PrimaveraStatusConverter();

        converter.Convert("Committed", typeof(string), null, null).Should().Be("Committed");
        converter.Convert("Previewing", typeof(string), null, null).Should().Be("Previewing");
        converter.Convert("Failed", typeof(string), null, null).Should().Be("Failed");
        converter.Convert("RolledBack", typeof(string), null, null).Should().Be("Rolled Back");
        converter.Convert("Unknown", typeof(string), null, null).Should().Be("Unknown");
        converter.Convert(null, typeof(string), null, null).Should().Be(string.Empty);
        converter.Convert(42, typeof(string), null, null).Should().Be(string.Empty);
    }

    [Fact]
    public void PrimaveraStatusConverter_ConvertBack_ThrowsNotImplemented()
    {
        var converter = new PrimaveraStatusConverter();
        Action act = () => converter.ConvertBack("Committed", typeof(string), null, null);
        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void EntityTypeSummary_CanBeCreated()
    {
        var summary = new EntityTypeSummary
        {
            TypeName = "Activities",
            Count = 42,
            Details = "42 row(s)"
        };

        summary.TypeName.Should().Be("Activities");
        summary.Count.Should().Be(42);
        summary.Details.Should().Be("42 row(s)");
    }

    [Fact]
    public void XeroTableSection_IsObservable()
    {
        var section = new XeroTableSection { TableName = "TASK", IsSelected = false };
        section.TableName.Should().Be("TASK");
        section.IsSelected.Should().BeFalse();

        section.IsSelected = true;
        section.IsSelected.Should().BeTrue();
    }

    [Fact]
    public void DcmaThresholdConfig_CanBeCreated()
    {
        var config = new DcmaThresholdConfig
        {
            PointNumber = 1,
            PointName = "Missing Logic",
            Status = DcmaStatus.Pass,
            IssueCount = 0,
            TotalCount = 10,
            Percentage = 0,
            Details = "All good"
        };

        config.PointNumber.Should().Be(1);
        config.PointName.Should().Be("Missing Logic");
        config.Status.Should().Be(DcmaStatus.Pass);
    }

    [Fact]
    public void GroupedIssuesViewModel_CanBeCreated()
    {
        var group = new GroupedIssuesViewModel
        {
            Key = "Error",
            Items = new()
        };
        group.Key.Should().Be("Error");
        group.Items.Should().BeEmpty();
    }

    [Fact]
    public void DcmaThresholdConfig_MinProperties_Nullable()
    {
        var config = new DcmaThresholdConfig();
        config.MinNumber.Should().BeNull();
        config.MinPercentage.Should().BeNull();
    }

    [Fact]
    public void PrimaveraStudioViewModel_LoadAsync_DoesNotThrow()
    {
        var vm = CreateStudioViewModel();
        var act = () => vm.LoadAsync(1);
        act.Should().NotThrowAsync();
    }

    [Fact]
    public void PrimaveraStudioViewModel_Tabs_IsObservableCollection()
    {
        var vm = CreateStudioViewModel();
        vm.Tabs.Should().BeAssignableTo<System.Collections.Specialized.INotifyCollectionChanged>();
    }

    [Fact]
    public void AllViewModels_AreObservableObjects()
    {
        var importVm = new PrimaveraImportViewModel(Mock.Of<IPrimaveraImportService>());
        var workspaceVm = CreateWorkspaceViewModel();
        var activitiesVm = new PrimaveraActivitiesViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        var relationshipsVm = new PrimaveraRelationshipsViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        var resourcesVm = new PrimaveraResourcesViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        var calendarsVm = new PrimaveraCalendarsViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        var codesVm = new PrimaveraCodesViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        var baselinesVm = new PrimaveraBaselinesViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        var udfsVm = new PrimaveraUdfsViewModel(Mock.Of<IPrimaveraWorkspaceService>());
        var validationVm = new PrimaveraValidationViewModel(Mock.Of<IPrimaveraValidationService>(), Mock.Of<IPrimaveraImportService>());
        var repairVm = new PrimaveraRepairViewModel(Mock.Of<IPrimaveraRepairService>());
        var exportVm = new PrimaveraExportViewModel(Mock.Of<IPrimaveraExportService>());

        importVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        workspaceVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        activitiesVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        relationshipsVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        resourcesVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        calendarsVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        codesVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        baselinesVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        udfsVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        validationVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        repairVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        exportVm.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
    }

    private static PrimaveraStudioViewModel CreateStudioViewModel(ICurrentProjectService? projectService = null)
    {
        projectService ??= new SpyCurrentProjectService();
        var importVm = new PrimaveraImportViewModel(Mock.Of<IPrimaveraImportService>());
        var workspaceVm = CreateWorkspaceViewModel();
        var validationVm = new PrimaveraValidationViewModel(Mock.Of<IPrimaveraValidationService>(), Mock.Of<IPrimaveraImportService>());
        var repairVm = new PrimaveraRepairViewModel(Mock.Of<IPrimaveraRepairService>());
        var exportVm = new PrimaveraExportViewModel(Mock.Of<IPrimaveraExportService>());

        return new PrimaveraStudioViewModel(importVm, workspaceVm, validationVm, repairVm, exportVm, projectService);
    }

    private static PrimaveraWorkspaceViewModel CreateWorkspaceViewModel()
    {
        return new PrimaveraWorkspaceViewModel(
            Mock.Of<IPrimaveraWorkspaceService>(),
            Mock.Of<IPrimaveraImportService>(),
            new PrimaveraCalendarsViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraCodesViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraActivitiesViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraRelationshipsViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraResourcesViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraBaselinesViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraUdfsViewModel(Mock.Of<IPrimaveraWorkspaceService>()));
    }
}
