using System.Windows.Controls;
using Planova.UI.ViewModels.Boq;

namespace Planova.UI.Views.Boq;

public partial class BoqEditorView : UserControl
{
    public BoqEditorView(BoqEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
