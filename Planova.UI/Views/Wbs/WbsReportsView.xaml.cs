using System.Windows.Controls;
using Planova.UI.ViewModels.Wbs;

namespace Planova.UI.Views.Wbs;

public partial class WbsReportsView : UserControl
{
    public WbsReportsView(WbsReportsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
