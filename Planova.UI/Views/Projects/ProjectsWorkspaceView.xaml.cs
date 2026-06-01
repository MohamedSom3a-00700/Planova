using System.Windows.Controls;
using Planova.UI.ViewModels;

namespace Planova.UI.Views.Projects;

public partial class ProjectsWorkspaceView : UserControl
{
    public ProjectsWorkspaceView(ProjectsWorkspaceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
