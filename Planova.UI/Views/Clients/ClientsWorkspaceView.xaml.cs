using System.Windows.Controls;
using Planova.UI.ViewModels;

namespace Planova.UI.Views.Clients;

public partial class ClientsWorkspaceView : UserControl
{
    public ClientsWorkspaceView(ClientsWorkspaceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
