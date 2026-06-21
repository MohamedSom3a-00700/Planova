using System.Windows;
using System.Windows.Controls;
using Planova.UI.ViewModels.Boq;

namespace Planova.UI.Views.Boq;

public partial class BoqTreeView : UserControl
{
    public BoqTreeView(BoqTreeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OutlineTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is BoqTreeViewModel vm && e.NewValue is BoqOutlineItem item)
        {
            vm.SelectedOutlineItem = item;
        }
    }
}
