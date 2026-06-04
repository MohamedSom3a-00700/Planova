using System.Windows.Controls;
using Planova.UI.ViewModels.Wbs;

namespace Planova.UI.Views.Wbs;

public partial class WbsTemplateManagerView : UserControl
{
    public WbsTemplateManagerView(WbsTemplateManagerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
