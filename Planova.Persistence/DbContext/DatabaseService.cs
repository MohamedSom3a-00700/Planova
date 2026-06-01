using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Planova.Persistence.DbContext;
using Planova.Shared.Abstractions;

namespace Planova.Persistence.Services;

public class DatabaseService : IDatabaseService
{
    private readonly PlanovaDbContext _context;
    private readonly ILoggingService _logger;
    private bool _initialized;

    public DatabaseService(PlanovaDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public object GetConnection() => _context;

    public bool IsInitialized() => _initialized;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        try
        {
            await _context.Database.MigrateAsync(ct);
            _initialized = true;
        }
        catch (SqliteException ex) when (IsSchemaConflict(ex))
        {
            _logger.Error($"Schema conflict during migration. Database may be in an inconsistent state. _initialized={_initialized}", ex);
            throw;
        }
    }

    private static bool IsSchemaConflict(SqliteException ex)
    {
        return ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase)
               || ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase)
               || ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }
}
