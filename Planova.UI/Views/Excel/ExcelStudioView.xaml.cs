using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Planova.UI.ViewModels.Excel;

namespace Planova.UI.Views.Excel;

public partial class ExcelStudioView : UserControl
{
    public ExcelStudioView(ExcelStudioViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        var vm = (ExcelStudioViewModel)DataContext;
        vm.Tabs.Add(new ExcelStudioTab("Browser", serviceProvider.GetRequiredService<WorkbookBrowserView>()));
        vm.Tabs.Add(new ExcelStudioTab("Import", serviceProvider.GetRequiredService<ImportWizardView>()));
        vm.Tabs.Add(new ExcelStudioTab("Export", serviceProvider.GetRequiredService<ExportWizardView>()));
        vm.Tabs.Add(new ExcelStudioTab("Profiles", serviceProvider.GetRequiredService<MappingProfilesView>()));
        vm.SelectedTab = vm.Tabs[0];
    }
}
