using System.Windows.Controls;
using Planova.UI.ViewModels;

namespace Planova.UI.Views.Dashboard;

public partial class DashboardView : UserControl
{
    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => viewModel.LoadCommand.Execute(null);
    }
}
