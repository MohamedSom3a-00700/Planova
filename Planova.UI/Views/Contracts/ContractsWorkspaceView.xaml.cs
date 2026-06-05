using System.Windows.Controls;
using Planova.UI.ViewModels;

namespace Planova.UI.Views.Contracts;

public partial class ContractsWorkspaceView : UserControl
{
    public ContractsWorkspaceView(ContractsWorkspaceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
