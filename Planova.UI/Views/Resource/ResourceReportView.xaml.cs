using System.Windows.Controls;
using Planova.UI.ViewModels.Resource;

namespace Planova.UI.Views.Resource;

public partial class ResourceReportView : UserControl
{
    public ResourceReportView(ResourceReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
