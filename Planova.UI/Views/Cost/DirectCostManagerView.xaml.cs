using System.Windows.Controls;
using Planova.UI.ViewModels.Cost;

namespace Planova.UI.Views.Cost;

public partial class DirectCostManagerView : UserControl
{
    public DirectCostManagerView(DirectCostManagerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
