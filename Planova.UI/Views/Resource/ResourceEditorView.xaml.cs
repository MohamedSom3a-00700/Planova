using System.Windows.Controls;
using Planova.UI.ViewModels.Resource;

namespace Planova.UI.Views.Resource;

public partial class ResourceEditorView : UserControl
{
    public ResourceEditorView(ResourceEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
