using System.Windows.Controls;
using Planova.UI.ViewModels.Parties;

namespace Planova.UI.Views.Parties;

public partial class PartyDetailView : UserControl
{
    public PartyDetailView(PartyDetailViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
