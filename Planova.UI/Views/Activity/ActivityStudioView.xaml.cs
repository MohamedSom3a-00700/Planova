using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Planova.UI.ViewModels.Activity;

namespace Planova.UI.Views.Activity;

public partial class ActivityStudioView : UserControl
{
    public ActivityStudioView(ActivityStudioViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        var vm = (ActivityStudioViewModel)DataContext;
        vm.Tabs.Add(new ActivityStudioTab("List", serviceProvider.GetRequiredService<ActivityListView>()));
        vm.Tabs.Add(new ActivityStudioTab("Gantt", serviceProvider.GetRequiredService<GanttChartView>()));
        vm.Tabs.Add(new ActivityStudioTab("Editor", serviceProvider.GetRequiredService<ActivityEditorView>()));
        vm.Tabs.Add(new ActivityStudioTab("Relationships", serviceProvider.GetRequiredService<RelationshipEditorView>()));
        vm.Tabs.Add(new ActivityStudioTab("Calendars", serviceProvider.GetRequiredService<CalendarManagerView>()));
        vm.Tabs.Add(new ActivityStudioTab("Activity Bank", serviceProvider.GetRequiredService<ActivityBankBrowserView>()));
        vm.Tabs.Add(new ActivityStudioTab("Generation", serviceProvider.GetRequiredService<WbsGenerationWizardView>()));
        vm.Tabs.Add(new ActivityStudioTab("Reports", serviceProvider.GetRequiredService<ScheduleReportView>()));
        vm.SelectedTab = vm.Tabs[0];
    }
}
