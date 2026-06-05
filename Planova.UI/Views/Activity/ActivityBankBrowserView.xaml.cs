using System.Windows.Controls;
using Planova.UI.ViewModels.Activity;

namespace Planova.UI.Views.Activity;

public partial class ActivityBankBrowserView : UserControl
{
    public ActivityBankBrowserView(ActivityBankBrowserViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
