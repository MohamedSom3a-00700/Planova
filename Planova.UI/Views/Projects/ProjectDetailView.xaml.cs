using System.Windows.Controls;
using Planova.UI.ViewModels.Projects;

namespace Planova.UI.Views.Projects;

public partial class ProjectDetailView : UserControl
{
    public ProjectDetailView()
    {
        InitializeComponent();
    }

    public ProjectDetailView(ProjectDetailViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
