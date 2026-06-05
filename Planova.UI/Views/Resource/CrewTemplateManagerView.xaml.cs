using System.Windows.Controls;
using Planova.UI.ViewModels.Resource;

namespace Planova.UI.Views.Resource;

public partial class CrewTemplateManagerView : UserControl
{
    public CrewTemplateManagerView(CrewTemplateManagerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
