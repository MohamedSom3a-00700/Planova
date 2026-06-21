using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Services;
using Planova.Shared.Abstractions;
using Planova.UI.ViewModels.Shared;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public BoqStudioTab(string header, string icon, object content)
    {
        _header = header;
        _icon = icon;
        _content = content;
    }
}

public partial class BoqStudioViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public DocumentValidationBannerViewModel DocumentValidationBanner { get; }

    [ObservableProperty]
    private BoqStudioTab? _selectedTab;

    public ObservableCollection<BoqStudioTab> Tabs { get; } = new();

    public BoqStudioViewModel(IProjectDocumentService projectDocumentService, ICurrentProjectService currentProjectService, INavigationService navigationService)
    {
        _navigationService = navigationService;
        DocumentValidationBanner = new DocumentValidationBannerViewModel(projectDocumentService, currentProjectService)
        {
            RequiredTypes = new[] { "Boq" }
        };
    }

    [RelayCommand]
    private void OpenInSchedule()
    {
        _navigationService?.NavigateTo("activity");
    }
}
