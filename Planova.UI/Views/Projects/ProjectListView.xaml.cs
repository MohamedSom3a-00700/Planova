using System.Windows.Controls;
using Planova.UI.ViewModels.Projects;

namespace Planova.UI.Views.Projects;

public partial class ProjectListView : UserControl
{
    public ProjectListView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ProjectListViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
