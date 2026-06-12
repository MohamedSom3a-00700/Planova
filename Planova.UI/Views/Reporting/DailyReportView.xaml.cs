using System.Windows.Controls;
using Planova.UI.ViewModels.Reporting;

namespace Planova.UI.Views.Reporting;

public partial class DailyReportView : UserControl
{
    public DailyReportView(DailyReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
