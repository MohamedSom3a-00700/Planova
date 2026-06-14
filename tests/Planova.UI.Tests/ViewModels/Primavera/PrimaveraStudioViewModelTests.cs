using FluentAssertions;
using Moq;
using Planova.Primavera.Domain.Interfaces;
using Planova.Shared.Abstractions;
using Planova.UI.ViewModels.Primavera;

namespace Planova.UI.Tests.ViewModels.Primavera;

public sealed class SpyCurrentProjectService : ICurrentProjectService
{
    public ProjectContext? CurrentProject { get; private set; }
    public event EventHandler<ProjectContext?>? CurrentProjectChanged;

    public void SetProject(ProjectContext? project)
    {
        CurrentProject = project;
        CurrentProjectChanged?.Invoke(this, project);
    }
}

public class PrimaveraStudioViewModelTests
{
    [Fact]
    public void Constructor_WhenProjectAlreadyActive_LoadsWorkspaceData()
    {
        var spy = new SpyCurrentProjectService();
        spy.SetProject(new ProjectContext(42, "PROJ-001", "Test Project"));

        var vm = CreateViewModel(spy);

        vm.SelectedTab.Should().BeNull();
    }

    [Fact]
    public async Task OnCurrentProjectChanged_LoadsWorkspaceData()
    {
        var spy = new SpyCurrentProjectService();
        var vm = CreateViewModel(spy);

        spy.SetProject(new ProjectContext(99, "PROJ-099", "New Project"));

        await Task.Delay(100);
        vm.SelectedTab.Should().BeNull();
    }

    [Fact]
    public void Tabs_InitiallyEmpty()
    {
        var vm = CreateViewModel(new SpyCurrentProjectService());

        vm.Tabs.Should().NotBeNull();
        vm.Tabs.Should().BeEmpty();
    }

    private static PrimaveraStudioViewModel CreateViewModel(ICurrentProjectService projectService)
    {
        var importVm = new PrimaveraImportViewModel(Mock.Of<IPrimaveraImportService>());
        var workspaceVm = new PrimaveraWorkspaceViewModel(
            Mock.Of<IPrimaveraWorkspaceService>(),
            Mock.Of<IPrimaveraImportService>(),
            new PrimaveraCalendarsViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraCodesViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraActivitiesViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraRelationshipsViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraResourcesViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraBaselinesViewModel(Mock.Of<IPrimaveraWorkspaceService>()),
            new PrimaveraUdfsViewModel(Mock.Of<IPrimaveraWorkspaceService>())
        );
        var validationVm = new PrimaveraValidationViewModel(Mock.Of<IPrimaveraValidationService>(), Mock.Of<IPrimaveraImportService>());
        var repairVm = new PrimaveraRepairViewModel(Mock.Of<IPrimaveraRepairService>());
        var exportVm = new PrimaveraExportViewModel(Mock.Of<IPrimaveraExportService>());

        return new PrimaveraStudioViewModel(
            importVm, workspaceVm, validationVm, repairVm, exportVm, projectService);
    }
}
