using System.Linq;
using System.Windows.Controls;
using Planova.Reporting.Application.Dto;
using Planova.UI.ViewModels.Reporting;

namespace Planova.UI.Views.Reporting;

public partial class ReportHistoryView : UserControl
{
    public ReportHistoryView(ReportHistoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void HistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ReportHistoryViewModel vm)
        {
            vm.SelectedInstances = HistoryGrid.SelectedItems.Cast<ReportInstanceDto>().ToList();
        }
    }
}
