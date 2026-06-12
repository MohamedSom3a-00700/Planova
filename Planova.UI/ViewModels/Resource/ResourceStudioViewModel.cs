using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Planova.Application.Services;
using Planova.Shared.Abstractions;
using Planova.UI.ViewModels.Shared;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public ResourceStudioTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class ResourceStudioViewModel : ObservableObject
{
    public DocumentValidationBannerViewModel DocumentValidationBanner { get; }

    [ObservableProperty]
    private ResourceStudioTab? _selectedTab;

    public ObservableCollection<ResourceStudioTab> Tabs { get; } = new();

    public ResourceStudioViewModel(IProjectDocumentService projectDocumentService, ICurrentProjectService currentProjectService)
    {
        DocumentValidationBanner = new DocumentValidationBannerViewModel(projectDocumentService, currentProjectService)
        {
            RequiredTypes = new[] { "Spec", "Boq" }
        };
    }
}
