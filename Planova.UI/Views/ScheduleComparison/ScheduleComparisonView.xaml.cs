using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Planova.UI.ViewModels.ScheduleComparison;

namespace Planova.UI.Views.ScheduleComparison;

public partial class ScheduleComparisonView : UserControl
{
    public ScheduleComparisonView(ScheduleComparisonViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        SetFlowDirection();
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        if (DataContext is ScheduleComparisonViewModel vm)
        {
            vm.InitializeTabs(serviceProvider);
        }
    }

    private void SetFlowDirection()
    {
        FlowDirection = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft
            ? System.Windows.FlowDirection.RightToLeft
            : System.Windows.FlowDirection.LeftToRight;
    }
}
