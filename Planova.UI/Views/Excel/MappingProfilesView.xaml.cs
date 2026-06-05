using System.Windows.Controls;
using Planova.UI.ViewModels.Excel;

namespace Planova.UI.Views.Excel;

public partial class MappingProfilesView : UserControl
{
    public MappingProfilesView(MappingProfilesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
