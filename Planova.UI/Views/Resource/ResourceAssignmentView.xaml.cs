using System.Windows.Controls;
using Planova.UI.ViewModels.Resource;

namespace Planova.UI.Views.Resource;

public partial class ResourceAssignmentView : UserControl
{
    public ResourceAssignmentView(ResourceAssignmentViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
