using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace Planova.UI.ViewModels;

public sealed partial class NavigationItemViewModel : ObservableObject
{
    public NavigationItemViewModel(string id, string displayName, ICommand command, string iconGlyph = "", bool isPlaceholder = false)
    {
        Id = id;
        DisplayName = displayName;
        Command = command;
        CommandParameter = id;
        IconGlyph = iconGlyph;
        IsPlaceholder = isPlaceholder;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public ICommand Command { get; }

    public string CommandParameter { get; }

    public string IconGlyph { get; }

    public bool IsPlaceholder { get; }

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isEnabled = true;
}
