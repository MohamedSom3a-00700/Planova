using System.Windows.Controls;
using Planova.UI.ViewModels.Boq;

namespace Planova.UI.Views.Boq;

public partial class BoqReportView : UserControl
{
    public BoqReportView(BoqReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
