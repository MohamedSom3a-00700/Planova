namespace Planova.UI.ViewModels.Wbs;

public enum WbsChangeType
{
    Created,
    Deleted,
    Updated
}

public record WbsStudioMessage(WbsChangeType ChangeType);
