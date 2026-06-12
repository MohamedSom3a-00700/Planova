using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using Planova.UI.ViewModels;

namespace Planova.UI.Views.Projects;

public partial class ProjectsWorkspaceView : UserControl
{
    private readonly ProjectsWorkspaceViewModel _viewModel;

    public ProjectsWorkspaceView(ProjectsWorkspaceViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        viewModel.CoordinatesFromMapClicked += OnCoordinatesFromMapClicked;
        Loaded += (_, _) => viewModel.LoadCommand.Execute(null);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectsWorkspaceViewModel.MapHtmlPath))
        {
            UpdateMapBrowser();
        }
    }

    private void UpdateMapBrowser()
    {
        if (!string.IsNullOrEmpty(_viewModel.MapHtmlPath) && System.IO.File.Exists(_viewModel.MapHtmlPath))
        {
            MapBrowser.Navigate(new Uri(_viewModel.MapHtmlPath));
        }
        else
        {
            MapBrowser.Navigate(new Uri("about:blank"));
        }
    }

    private void OnCoordinatesFromMapClicked(object? sender, string? e)
    {
        try
        {
            var result = MapBrowser.InvokeScript("getClickedCoords");
            if (result != null)
            {
                var parts = result.ToString()?.Split(',');
                if (parts != null && parts.Length == 2 &&
                    double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
                    double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
                {
                    _viewModel.EditLatitude = lat;
                    _viewModel.EditLongitude = lng;
                }
            }
        }
        catch
        {
        }
    }
}
