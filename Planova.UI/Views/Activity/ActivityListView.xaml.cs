using System.Windows.Controls;
using Planova.UI.ViewModels.Activity;

namespace Planova.UI.Views.Activity;

public partial class ActivityListView : UserControl
{
    public ActivityListView(ActivityListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
