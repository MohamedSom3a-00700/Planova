using System.Windows.Controls;
using Planova.UI.ViewModels.Cost;

namespace Planova.UI.Views.Cost;

public partial class BudgetRevisionView : UserControl
{
    public BudgetRevisionView(BudgetRevisionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
