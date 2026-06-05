using System.Windows;
using Planova.Shared.Abstractions;

namespace Planova.UI.Services;

public class HighContrastDetector : IHighContrastDetector, IDisposable
{
    public bool IsHighContrast => SystemParameters.HighContrast;

    public event EventHandler<bool>? HighContrastChanged;

    private bool _disposed;

    public HighContrastDetector()
    {
        SystemParameters.StaticPropertyChanged += OnStaticPropertyChanged;
    }

    private void OnStaticPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.HighContrast))
        {
            HighContrastChanged?.Invoke(this, SystemParameters.HighContrast);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            SystemParameters.StaticPropertyChanged -= OnStaticPropertyChanged;
            _disposed = true;
        }
    }
}