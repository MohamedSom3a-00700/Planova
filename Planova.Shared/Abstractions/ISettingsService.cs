namespace Planova.Shared.Abstractions;

public interface ISettingsService
{
    Task Load();
    Task Save();
    T? Get<T>(string key);
    void Set<T>(string key, T value);
}
