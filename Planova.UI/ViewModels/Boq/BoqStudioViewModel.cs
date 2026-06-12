using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Planova.Application.Services;
using Planova.Shared.Abstractions;
using Planova.UI.ViewModels.Shared;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public BoqStudioTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class BoqStudioViewModel : ObservableObject
{
    public DocumentValidationBannerViewModel DocumentValidationBanner { get; }

    [ObservableProperty]
    private BoqStudioTab? _selectedTab;

    public ObservableCollection<BoqStudioTab> Tabs { get; } = new();

    public BoqStudioViewModel(IProjectDocumentService projectDocumentService, ICurrentProjectService currentProjectService)
    {
        DocumentValidationBanner = new DocumentValidationBannerViewModel(projectDocumentService, currentProjectService)
        {
            RequiredTypes = new[] { "Boq" }
        };
    }
}
