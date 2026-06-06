using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Planova.Application.Dto;
using Planova.UI.ViewModels;

namespace Planova.UI.Views;

public partial class NavigationRail : UserControl, INotifyPropertyChanged
{
    private bool _isCollapsed;

    public event PropertyChangedEventHandler? PropertyChanged;

    public NavigationRail()
    {
        InitializeComponent();
    }

    private void OnProjectSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ProjectSummaryDto project)
        {
            var vm = DataContext as ShellViewModel;
            vm?.SelectProjectCommand.Execute(project);
        }
    }

    public bool IsCollapsed
    {
        get => _isCollapsed;
        private set
        {
            if (_isCollapsed == value) return;
            _isCollapsed = value;
            OnPropertyChanged();
        }
    }

    private void OnToggleClick(object sender, RoutedEventArgs e)
    {
        IsCollapsed = !IsCollapsed;
        Width = IsCollapsed ? 72 : 232;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
