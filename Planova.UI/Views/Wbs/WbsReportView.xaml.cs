using System.Windows.Controls;
using Planova.UI.ViewModels.Wbs;

namespace Planova.UI.Views.Wbs;

public partial class WbsReportView : UserControl
{
    public WbsReportView(WbsReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
