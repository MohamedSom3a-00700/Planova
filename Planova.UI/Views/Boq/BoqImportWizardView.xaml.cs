using System.Windows.Controls;
using Planova.UI.ViewModels.Boq;

namespace Planova.UI.Views.Boq;

public partial class BoqImportWizardView : UserControl
{
    public BoqImportWizardView(BoqImportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
