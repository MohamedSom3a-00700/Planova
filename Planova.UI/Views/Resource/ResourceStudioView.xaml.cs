using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Planova.UI.ViewModels.Resource;

namespace Planova.UI.Views.Resource;

public partial class ResourceStudioView : UserControl
{
    public ResourceStudioView(ResourceStudioViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        var vm = (ResourceStudioViewModel)DataContext;
        vm.Tabs.Add(new ResourceStudioTab("Library", serviceProvider.GetRequiredService<ResourceLibraryView>()));
        vm.Tabs.Add(new ResourceStudioTab("Rates", serviceProvider.GetRequiredService<ResourceRateManagerView>()));
        vm.Tabs.Add(new ResourceStudioTab("Crew Templates", serviceProvider.GetRequiredService<CrewTemplateManagerView>()));
        vm.Tabs.Add(new ResourceStudioTab("Assignments", serviceProvider.GetRequiredService<ResourceAssignmentView>()));
        vm.Tabs.Add(new ResourceStudioTab("Histogram", serviceProvider.GetRequiredService<ResourceHistogramView>()));
        vm.Tabs.Add(new ResourceStudioTab("AI Estimation", serviceProvider.GetRequiredService<ResourceAiEstimationView>()));
        vm.Tabs.Add(new ResourceStudioTab("Reports", serviceProvider.GetRequiredService<ResourceReportView>()));
        vm.Tabs.Add(new ResourceStudioTab("Settings", serviceProvider.GetRequiredService<ResourceSettingsView>()));
        vm.SelectedTab = vm.Tabs[0];
    }
}
