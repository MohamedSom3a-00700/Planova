using System.Windows.Controls;
using Planova.UI.ViewModels.Cost;

namespace Planova.UI.Views.Cost;

public partial class CostAiView : UserControl
{
    public CostAiView(CostAiViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
