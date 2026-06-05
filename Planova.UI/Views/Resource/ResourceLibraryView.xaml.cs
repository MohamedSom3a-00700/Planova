using System.Windows.Controls;
using Planova.UI.ViewModels.Resource;

namespace Planova.UI.Views.Resource;

public partial class ResourceLibraryView : UserControl
{
    public ResourceLibraryView(ResourceLibraryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
