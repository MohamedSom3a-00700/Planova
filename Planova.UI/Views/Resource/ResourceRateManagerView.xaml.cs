using System.Windows.Controls;
using Planova.UI.ViewModels.Resource;

namespace Planova.UI.Views.Resource;

public partial class ResourceRateManagerView : UserControl
{
    public ResourceRateManagerView(ResourceRateManagerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
