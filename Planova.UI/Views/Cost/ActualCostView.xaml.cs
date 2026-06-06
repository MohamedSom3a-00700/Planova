using System.Windows.Controls;
using Planova.UI.ViewModels.Cost;

namespace Planova.UI.Views.Cost;

public partial class ActualCostView : UserControl
{
    public ActualCostView(ActualCostViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
