using System.Windows.Controls;
using Planova.UI.ViewModels.Wbs;

namespace Planova.UI.Views.Wbs;

public partial class WbsListView : UserControl
{
    public WbsListView(WbsListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
