using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Planova.UI.ViewModels.Wbs;

namespace Planova.UI.Views.Wbs;

public partial class WbsStudioView : UserControl
{
    public WbsStudioView(WbsStudioViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        var vm = (WbsStudioViewModel)DataContext;
        vm.Tabs.Add(new WbsStudioTab("List", serviceProvider.GetRequiredService<WbsListView>()));
        vm.Tabs.Add(new WbsStudioTab("Tree", serviceProvider.GetRequiredService<WbsTreeView>()));
        vm.Tabs.Add(new WbsStudioTab("Editor", serviceProvider.GetRequiredService<WbsEditorView>()));
        vm.Tabs.Add(new WbsStudioTab("Mapping", serviceProvider.GetRequiredService<WbsMappingWizardView>()));
        vm.Tabs.Add(new WbsStudioTab("Templates", serviceProvider.GetRequiredService<WbsTemplateManagerView>()));
        vm.Tabs.Add(new WbsStudioTab("AI Generation", serviceProvider.GetRequiredService<WbsAiGenerationView>()));
        vm.Tabs.Add(new WbsStudioTab("Reports", serviceProvider.GetRequiredService<WbsReportView>()));
        vm.Tabs.Add(new WbsStudioTab("Settings", serviceProvider.GetRequiredService<WbsSettingsView>()));
        vm.SelectedTab = vm.Tabs[0];
    }
}
