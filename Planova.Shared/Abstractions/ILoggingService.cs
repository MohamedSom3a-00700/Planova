namespace Planova.Shared.Abstractions;

public interface ILoggingService
{
    void Info(string message, params object[] args);
    void Error(string message, Exception ex, params object[] args);
    void Warning(string message, params object[] args);
}
