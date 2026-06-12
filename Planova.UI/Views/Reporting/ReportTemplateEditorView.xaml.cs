using System.Windows.Controls;
using Planova.UI.ViewModels.Reporting;

namespace Planova.UI.Views.Reporting;

public partial class ReportTemplateEditorView : UserControl
{
    public ReportTemplateEditorView(ReportTemplateEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
