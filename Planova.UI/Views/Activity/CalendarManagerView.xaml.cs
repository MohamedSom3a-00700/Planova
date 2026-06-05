using System.Windows.Controls;
using Planova.UI.ViewModels.Activity;

namespace Planova.UI.Views.Activity;

public partial class CalendarManagerView : UserControl
{
    public CalendarManagerView(CalendarManagerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
