using System.Windows.Controls;
using Planova.UI.ViewModels.Boq;

namespace Planova.UI.Views.Boq;

public partial class BoqLibraryView : UserControl
{
    public BoqLibraryView(BoqLibraryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
