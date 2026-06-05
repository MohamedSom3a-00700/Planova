using System.Windows.Controls;
using Planova.UI.ViewModels.Activity;

namespace Planova.UI.Views.Activity;

public partial class RelationshipEditorView : UserControl
{
    public RelationshipEditorView(RelationshipEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
