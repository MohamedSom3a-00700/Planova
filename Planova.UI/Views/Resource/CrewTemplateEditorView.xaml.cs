using System.Windows.Controls;
using Planova.UI.ViewModels.Resource;

namespace Planova.UI.Views.Resource;

public partial class CrewTemplateEditorView : UserControl
{
    public CrewTemplateEditorView(CrewTemplateEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
