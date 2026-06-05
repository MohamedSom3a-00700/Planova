namespace Planova.Shared.Abstractions;

public interface IDatabaseService
{
    Task InitializeAsync(CancellationToken ct = default);
    object GetConnection();
    bool IsInitialized();
}
