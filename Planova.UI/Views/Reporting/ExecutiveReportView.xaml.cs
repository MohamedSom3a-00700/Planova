using System.Windows.Controls;
using Planova.UI.ViewModels.Reporting;

namespace Planova.UI.Views.Reporting;

public partial class ExecutiveReportView : UserControl
{
    public ExecutiveReportView(ExecutiveReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
