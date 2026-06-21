using System.Windows.Controls;
using Planova.UI.ViewModels;

namespace Planova.UI.Views.Projects;

public partial class ProjectsWorkspaceView : UserControl
{
    private readonly ProjectsWorkspaceViewModel _viewModel;

    public ProjectsWorkspaceView(ProjectsWorkspaceViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => viewModel.LoadCommand.Execute(null);
    }

    private void OnNewProjectClick(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.NewProject();
    }
}
