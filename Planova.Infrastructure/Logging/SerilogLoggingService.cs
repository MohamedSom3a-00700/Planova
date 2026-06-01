using Planova.Shared.Abstractions;
using Serilog;

namespace Planova.Infrastructure.Logging;

public class SerilogLoggingService : ILoggingService
{
    public void Info(string message, params object[] args)
    {
        Log.Information(message, args);
    }

    public void Error(string message, Exception ex, params object[] args)
    {
        Log.Error(ex, message, args);
    }

    public void Warning(string message, params object[] args)
    {
        Log.Warning(message, args);
    }
}
