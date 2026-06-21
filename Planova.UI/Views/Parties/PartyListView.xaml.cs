using System.Windows.Controls;
using Planova.UI.ViewModels.Parties;

namespace Planova.UI.Views.Parties;

public partial class PartyListView : UserControl
{
    public PartyListView(PartyListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => viewModel.LoadCommand.Execute(null);
    }
}
