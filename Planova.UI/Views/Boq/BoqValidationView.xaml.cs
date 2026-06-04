using System.Windows.Controls;
using Planova.UI.ViewModels.Boq;

namespace Planova.UI.Views.Boq;

public partial class BoqValidationView : UserControl
{
    public BoqValidationView(BoqValidationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
