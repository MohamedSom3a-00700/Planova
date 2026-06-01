using System.Windows.Controls;
using Planova.UI.ViewModels;

namespace Planova.UI.Views.Reports;

public partial class ReportView : UserControl
{
    public ReportView(ReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
