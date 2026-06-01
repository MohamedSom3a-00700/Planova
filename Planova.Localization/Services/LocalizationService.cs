using System.Globalization;
using System.Resources;
using Planova.Shared.Abstractions;

namespace Planova.Localization.Services;

public class LocalizationService : ILocalizationService
{
    private string _currentLanguage = "en";
    private readonly ResourceManager _resourceManager;

    public LocalizationService()
    {
        _resourceManager = new ResourceManager("Planova.Localization.Resources.Strings",
            typeof(LocalizationService).Assembly);
    }

    public string GetCurrentLanguage() => _currentLanguage;

    public string GetString(string key)
    {
        try
        {
            var culture = new CultureInfo(_currentLanguage == "ar" ? "ar-SA" : "en-US");
            return _resourceManager.GetString(key, culture) ?? key;
        }
        catch
        {
            return key;
        }
    }

    public bool IsRtl() => _currentLanguage == "ar";

    public void SetLanguage(string cultureCode)
    {
        if (_currentLanguage == cultureCode)
            return;

        _currentLanguage = cultureCode;
        LanguageChanged?.Invoke(this, cultureCode);
    }

    public event EventHandler<string>? LanguageChanged;
}
