using System.Windows.Controls;
using Planova.UI.ViewModels.Cost;

namespace Planova.UI.Views.Cost;

public partial class EvmView : UserControl
{
    public EvmView(EvmViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
