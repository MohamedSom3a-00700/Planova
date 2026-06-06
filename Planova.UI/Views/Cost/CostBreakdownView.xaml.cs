using System.Windows;
using System.Windows.Controls;
using Planova.UI.ViewModels.Cost;

namespace Planova.UI.Views.Cost;

public partial class CostBreakdownView : UserControl
{
    public CostBreakdownView(CostBreakdownViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is CostBreakdownViewModel vm && e.NewValue is not null)
        {
            // Handle selection change
        }
    }
}
