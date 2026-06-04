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
}
