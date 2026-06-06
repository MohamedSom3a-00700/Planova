using System.Windows.Controls;
using Planova.UI.ViewModels.Cost;

namespace Planova.UI.Views.Cost;

public partial class CashFlowView : UserControl
{
    public CashFlowView(CashFlowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
