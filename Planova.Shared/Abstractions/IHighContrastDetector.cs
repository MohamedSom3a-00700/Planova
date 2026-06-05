namespace Planova.Shared.Abstractions;

public interface IHighContrastDetector
{
    bool IsHighContrast { get; }
    event EventHandler<bool> HighContrastChanged;
}