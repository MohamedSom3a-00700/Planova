using System.Windows.Controls;
using Planova.UI.ViewModels.Cost;

namespace Planova.UI.Views.Cost;

public partial class BudgetView : UserControl
{
    public BudgetView(BudgetViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
