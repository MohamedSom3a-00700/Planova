using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Planova.UI.ViewModels.Boq;

namespace Planova.UI.Views.Boq;

public partial class BoqStudioView : UserControl
{
    public BoqStudioView(BoqStudioViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        var vm = (BoqStudioViewModel)DataContext;
        vm.Tabs.Add(new BoqStudioTab("Tree", serviceProvider.GetRequiredService<BoqTreeView>()));
        vm.Tabs.Add(new BoqStudioTab("Import", serviceProvider.GetRequiredService<BoqImportWizardView>()));
        vm.Tabs.Add(new BoqStudioTab("Editor", serviceProvider.GetRequiredService<BoqEditorView>()));
        vm.Tabs.Add(new BoqStudioTab("Validate", serviceProvider.GetRequiredService<BoqValidationView>()));
        vm.Tabs.Add(new BoqStudioTab("Classification", serviceProvider.GetRequiredService<BoqClassificationView>()));
        vm.Tabs.Add(new BoqStudioTab("Libraries", serviceProvider.GetRequiredService<BoqLibraryView>()));
        vm.Tabs.Add(new BoqStudioTab("Reports", serviceProvider.GetRequiredService<BoqReportView>()));
        vm.Tabs.Add(new BoqStudioTab("Settings", serviceProvider.GetRequiredService<BoqSettingsView>()));
        vm.SelectedTab = vm.Tabs[0];
    }
}
