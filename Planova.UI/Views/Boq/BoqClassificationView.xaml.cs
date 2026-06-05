using System.Windows.Controls;
using Planova.UI.ViewModels.Boq;

namespace Planova.UI.Views.Boq;

public partial class BoqClassificationView : UserControl
{
    public BoqClassificationView(BoqClassificationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
