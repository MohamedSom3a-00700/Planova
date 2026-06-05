using System.Windows.Controls;
using Planova.UI.ViewModels.Activity;

namespace Planova.UI.Views.Activity;

public partial class WbsGenerationWizardView : UserControl
{
    public WbsGenerationWizardView(WbsGenerationWizardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
