using System.Windows.Controls;
using Planova.UI.ViewModels.Excel;

namespace Planova.UI.Views.Excel;

public partial class ImportWizardView : UserControl
{
    public ImportWizardView(ImportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
