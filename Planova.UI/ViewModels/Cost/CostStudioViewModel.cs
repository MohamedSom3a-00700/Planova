using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Planova.Application.Services;
using Planova.Shared.Abstractions;
using Planova.UI.ViewModels.Shared;

namespace Planova.UI.ViewModels.Cost;

public partial class CostStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public CostStudioTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class CostStudioViewModel : ObservableObject
{
    public DocumentValidationBannerViewModel DocumentValidationBanner { get; }

    [ObservableProperty]
    private CostStudioTab? _selectedTab;

    public ObservableCollection<CostStudioTab> Tabs { get; } = new();

    public CostStudioViewModel(IProjectDocumentService projectDocumentService, ICurrentProjectService currentProjectService)
    {
        DocumentValidationBanner = new DocumentValidationBannerViewModel(projectDocumentService, currentProjectService)
        {
            RequiredTypes = new[] { "Boq", "Contract" }
        };
    }
}
