using System.Windows.Controls;
using Planova.UI.ViewModels.Reporting;

namespace Planova.UI.Views.Reporting;

public partial class WeeklyReportView : UserControl
{
    public WeeklyReportView(WeeklyReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
