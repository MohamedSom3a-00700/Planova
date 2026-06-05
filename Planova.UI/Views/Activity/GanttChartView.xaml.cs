using System.Windows.Controls;
using Planova.UI.ViewModels.Activity;

namespace Planova.UI.Views.Activity;

public partial class GanttChartView : UserControl
{
    public GanttChartView(GanttChartViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
