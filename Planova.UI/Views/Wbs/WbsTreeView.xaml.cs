using System.Windows.Controls;
using Planova.UI.ViewModels.Wbs;

namespace Planova.UI.Views.Wbs;

public partial class WbsTreeView : UserControl
{
    public WbsTreeView(WbsTreeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
