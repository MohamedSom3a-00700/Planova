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
        vm.Tabs.Add(new BoqStudioTab("Outline", "DocumentBulletList24", serviceProvider.GetRequiredService<BoqTreeView>()));
        vm.Tabs.Add(new BoqStudioTab("Import", "ArrowUpload24", serviceProvider.GetRequiredService<BoqImportWizardView>()));
        vm.Tabs.Add(new BoqStudioTab("Classification", "Classification24", serviceProvider.GetRequiredService<BoqClassificationView>()));
        vm.Tabs.Add(new BoqStudioTab("Reports", "DocumentText24", serviceProvider.GetRequiredService<BoqReportView>()));
        vm.Tabs.Add(new BoqStudioTab("Settings", "Settings24", serviceProvider.GetRequiredService<BoqSettingsView>()));
        vm.SelectedTab = vm.Tabs[0];
    }
}
