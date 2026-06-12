using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Planova.Application.Services;
using Planova.Shared.Abstractions;
using Planova.UI.ViewModels.Shared;

namespace Planova.UI.ViewModels.Wbs;

public partial class WbsStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public WbsStudioTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class WbsStudioViewModel : ObservableObject
{
    public DocumentValidationBannerViewModel DocumentValidationBanner { get; }

    [ObservableProperty]
    private WbsStudioTab? _selectedTab;

    public ObservableCollection<WbsStudioTab> Tabs { get; } = new();

    public WbsStudioViewModel(IProjectDocumentService projectDocumentService, ICurrentProjectService currentProjectService)
    {
        DocumentValidationBanner = new DocumentValidationBannerViewModel(projectDocumentService, currentProjectService)
        {
            RequiredTypes = new[] { "Boq", "Spec" }
        };
    }
}
