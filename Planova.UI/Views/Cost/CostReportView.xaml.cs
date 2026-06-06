using System.Windows.Controls;
using Planova.UI.ViewModels.Cost;

namespace Planova.UI.Views.Cost;

public partial class CostReportView : UserControl
{
    public CostReportView(CostReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
