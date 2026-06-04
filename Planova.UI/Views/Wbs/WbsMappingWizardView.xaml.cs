using System.Windows.Controls;
using Planova.UI.ViewModels.Wbs;

namespace Planova.UI.Views.Wbs;

public partial class WbsMappingWizardView : UserControl
{
    public WbsMappingWizardView(WbsMappingViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
