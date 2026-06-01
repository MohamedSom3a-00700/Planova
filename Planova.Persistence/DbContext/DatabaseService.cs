using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Shared.Abstractions;

namespace Planova.Persistence.Services;

public class DatabaseService : IDatabaseService
{
    private readonly PlanovaDbContext _context;
    private bool _initialized;

    public DatabaseService(PlanovaDbContext context)
    {
        _context = context;
    }

    public object GetConnection() => _context;

    public bool IsInitialized() => _initialized;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await _context.Database.EnsureCreatedAsync(ct);
        _initialized = true;
    }
}
