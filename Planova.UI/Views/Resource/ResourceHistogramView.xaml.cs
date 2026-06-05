using System.Windows.Controls;
using Planova.UI.ViewModels.Resource;

namespace Planova.UI.Views.Resource;

public partial class ResourceHistogramView : UserControl
{
    public ResourceHistogramView(ResourceHistogramViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
