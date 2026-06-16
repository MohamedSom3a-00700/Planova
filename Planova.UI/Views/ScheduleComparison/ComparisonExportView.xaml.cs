using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Planova.UI.ViewModels.ScheduleComparison;

namespace Planova.UI.Views.ScheduleComparison;

public partial class ComparisonExportView : UserControl
{
    public ComparisonExportView()
    {
        InitializeComponent();
        SetFlowDirection();
    }

    private void SetFlowDirection()
    {
        FlowDirection = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft
            ? System.Windows.FlowDirection.RightToLeft
            : System.Windows.FlowDirection.LeftToRight;
    }
}
