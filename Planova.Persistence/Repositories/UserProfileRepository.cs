using Microsoft.EntityFrameworkCore;
using Planova.Application.Repositories;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly PlanovaDbContext _context;

    public UserProfileRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<UserPreferences?> GetAsync(CancellationToken ct = default)
    {
        return await _context.UserPreferences.FirstOrDefaultAsync(ct);
    }

    public async Task<UserPreferences> SaveAsync(UserPreferences preferences, CancellationToken ct = default)
    {
        if (preferences.Id == 0)
            _context.UserPreferences.Add(preferences);

        await _context.SaveChangesAsync(ct);
        return preferences;
    }
}
