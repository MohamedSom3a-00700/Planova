using System.Windows.Controls;
using Planova.UI.ViewModels.Wbs;

namespace Planova.UI.Views.Wbs;

public partial class WbsAiGenerationView : UserControl
{
    public WbsAiGenerationView(WbsAiGenerationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
