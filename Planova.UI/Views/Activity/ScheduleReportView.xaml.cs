using System.Windows.Controls;
using Planova.UI.ViewModels.Activity;

namespace Planova.UI.Views.Activity;

public partial class ScheduleReportView : UserControl
{
    public ScheduleReportView(ScheduleReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
