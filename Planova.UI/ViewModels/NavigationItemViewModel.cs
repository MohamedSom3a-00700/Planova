using System.Windows.Input;

namespace Planova.UI.ViewModels;

public sealed class NavigationItemViewModel
{
    public NavigationItemViewModel(string id, string displayName, ICommand command)
    {
        Id = id;
        DisplayName = displayName;
        Command = command;
        CommandParameter = id;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public ICommand Command { get; }

    public string CommandParameter { get; }
}
