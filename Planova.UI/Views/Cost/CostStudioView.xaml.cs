using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Planova.UI.ViewModels.Cost;

namespace Planova.UI.Views.Cost;

public partial class CostStudioView : UserControl
{
    public CostStudioView(CostStudioViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        var vm = (CostStudioViewModel)DataContext;
        vm.Tabs.Add(new CostStudioTab("Cost Breakdown", serviceProvider.GetRequiredService<CostBreakdownView>()));
        vm.Tabs.Add(new CostStudioTab("Direct Costs", serviceProvider.GetRequiredService<DirectCostManagerView>()));
        vm.Tabs.Add(new CostStudioTab("Budget", serviceProvider.GetRequiredService<BudgetView>()));
        vm.Tabs.Add(new CostStudioTab("Actual Costs", serviceProvider.GetRequiredService<ActualCostView>()));
        vm.Tabs.Add(new CostStudioTab("Cash Flow", serviceProvider.GetRequiredService<CashFlowView>()));
        vm.Tabs.Add(new CostStudioTab("EVM", serviceProvider.GetRequiredService<EvmView>()));
        vm.Tabs.Add(new CostStudioTab("AI Services", serviceProvider.GetRequiredService<CostAiView>()));
        vm.Tabs.Add(new CostStudioTab("Reports", serviceProvider.GetRequiredService<CostReportView>()));
        vm.SelectedTab = vm.Tabs[0];
    }
}
