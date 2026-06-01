using System.Windows;
using System.Windows.Controls;

namespace Planova.UI.Views;

public partial class NavigationRail : UserControl
{
    private bool _isCollapsed;

    public NavigationRail()
    {
        InitializeComponent();
    }

    private void OnToggleClick(object sender, RoutedEventArgs e)
    {
        _isCollapsed = !_isCollapsed;
        Width = _isCollapsed ? 48 : 200;
    }
}
