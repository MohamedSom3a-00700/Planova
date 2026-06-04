using System.Windows.Controls;
using Planova.UI.ViewModels.Wbs;

namespace Planova.UI.Views.Wbs;

public partial class WbsEditorView : UserControl
{
    public WbsEditorView(WbsEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
