using System.Windows.Controls;
using Planova.UI.ViewModels.Activity;

namespace Planova.UI.Views.Activity;

public partial class ActivityEditorView : UserControl
{
    public ActivityEditorView(ActivityEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
