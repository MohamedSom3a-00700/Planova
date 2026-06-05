using System.Windows.Controls;
using Planova.UI.ViewModels.Excel;

namespace Planova.UI.Views.Excel;

public partial class ExportWizardView : UserControl
{
    public ExportWizardView(ExportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
