using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Planova.UI.ViewModels.Reporting;

namespace Planova.UI.Views.Reporting;

public partial class ReportingHubView : UserControl
{
    public ReportingHubView(ReportingHubViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        var vm = (ReportingHubViewModel)DataContext;

        Action<string> forward = msg => vm.OnHubStatusMessage?.Invoke(msg);

        var dailyView = serviceProvider.GetRequiredService<DailyReportView>();
        ((DailyReportViewModel)dailyView.DataContext).OnStatusMessage = forward;

        var weeklyView = serviceProvider.GetRequiredService<WeeklyReportView>();
        ((WeeklyReportViewModel)weeklyView.DataContext).OnStatusMessage = forward;

        var monthlyView = serviceProvider.GetRequiredService<MonthlyReportView>();
        ((MonthlyReportViewModel)monthlyView.DataContext).OnStatusMessage = forward;

        var executiveView = serviceProvider.GetRequiredService<ExecutiveReportView>();
        ((ExecutiveReportViewModel)executiveView.DataContext).OnStatusMessage = forward;

        var scheduleView = serviceProvider.GetRequiredService<ReportScheduleView>();
        ((ReportScheduleViewModel)scheduleView.DataContext).OnStatusMessage = forward;

        var historyView = serviceProvider.GetRequiredService<ReportHistoryView>();
        ((ReportHistoryViewModel)historyView.DataContext).OnStatusMessage = forward;

        var templateView = serviceProvider.GetRequiredService<ReportTemplateEditorView>();
        ((ReportTemplateEditorViewModel)templateView.DataContext).OnStatusMessage = forward;

        var settingsView = serviceProvider.GetRequiredService<ReportSettingsView>();
        ((ReportSettingsViewModel)settingsView.DataContext).OnStatusMessage = forward;

        vm.Tabs.Add(new ReportingHubTab("Daily", dailyView));
        vm.Tabs.Add(new ReportingHubTab("Weekly", weeklyView));
        vm.Tabs.Add(new ReportingHubTab("Monthly", monthlyView));
        vm.Tabs.Add(new ReportingHubTab("Executive", executiveView));
        vm.Tabs.Add(new ReportingHubTab("Schedule", scheduleView));
        vm.Tabs.Add(new ReportingHubTab("History", historyView));
        vm.Tabs.Add(new ReportingHubTab("Templates", templateView));
        vm.Tabs.Add(new ReportingHubTab("Settings", settingsView));
        vm.SelectedTab = vm.Tabs[0];
    }
}
