using Microsoft.Extensions.DependencyInjection;
using Planova.UI.ViewModels.Primavera;

namespace Planova.UI.Views.Primavera;

public partial class PrimaveraStudioView
{
    public PrimaveraStudioView(PrimaveraStudioViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        var vm = (PrimaveraStudioViewModel)DataContext;

        var importView = serviceProvider.GetRequiredService<PrimaveraImportView>();
        importView.DataContext = vm.ImportViewModel;
        vm.Tabs.Add(new PrimaveraStudioTab("Import", importView));

        var workspaceView = serviceProvider.GetRequiredService<PrimaveraWorkspaceView>();
        workspaceView.DataContext = vm.WorkspaceViewModel;
        vm.Tabs.Add(new PrimaveraStudioTab("Workspace", workspaceView));

        var validationView = serviceProvider.GetRequiredService<PrimaveraValidationView>();
        validationView.DataContext = vm.ValidationViewModel;
        vm.Tabs.Add(new PrimaveraStudioTab("Validate", validationView));

        var repairView = serviceProvider.GetRequiredService<PrimaveraRepairView>();
        repairView.DataContext = vm.RepairViewModel;
        vm.Tabs.Add(new PrimaveraStudioTab("Repair", repairView));

        var exportView = serviceProvider.GetRequiredService<PrimaveraExportView>();
        exportView.DataContext = vm.ExportViewModel;
        vm.Tabs.Add(new PrimaveraStudioTab("Export", exportView));

        if (vm.Tabs.Count > 0)
            vm.SelectedTab = vm.Tabs[0];
    }
}
