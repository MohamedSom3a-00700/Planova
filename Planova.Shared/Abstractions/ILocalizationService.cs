namespace Planova.Shared.Abstractions;

public interface ILocalizationService
{
    void SetLanguage(string cultureCode);
    string GetString(string key);
    string GetCurrentLanguage();
    bool IsRtl();
    event EventHandler<string> LanguageChanged;
}
