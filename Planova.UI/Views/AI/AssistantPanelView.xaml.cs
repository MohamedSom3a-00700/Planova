using System.Windows;
using System.Windows.Controls;
using Planova.UI.ViewModels;

namespace Planova.UI.Views.AI;

public partial class AssistantPanelView : UserControl
{
    public AssistantPanelView()
    {
        InitializeComponent();
    }

    private void OnCollapsedClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is AssistantPanelViewModel vm && !vm.IsExpanded)
        {
            vm.ToggleExpandCommand.Execute(null);
        }
    }
}