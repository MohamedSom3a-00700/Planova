using System.Windows.Controls;
using Planova.UI.ViewModels.Resource;

namespace Planova.UI.Views.Resource;

public partial class ResourceAiEstimationView : UserControl
{
    public ResourceAiEstimationView(ResourceAiEstimationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
