using System.Windows.Controls;
using Planova.UI.ViewModels.Reporting;

namespace Planova.UI.Views.Reporting;

public partial class MonthlyReportView : UserControl
{
    public MonthlyReportView(MonthlyReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
