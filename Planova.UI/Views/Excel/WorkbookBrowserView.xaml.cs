using System.Windows.Controls;
using Planova.UI.ViewModels.Excel;

namespace Planova.UI.Views.Excel;

public partial class WorkbookBrowserView : UserControl
{
    public WorkbookBrowserView(WorkbookBrowserViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
