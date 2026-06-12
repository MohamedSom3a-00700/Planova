using System.Windows.Controls;
using Planova.UI.ViewModels.Reporting;

namespace Planova.UI.Views.Reporting;

public partial class ReportScheduleView : UserControl
{
    public ReportScheduleView(ReportScheduleViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
