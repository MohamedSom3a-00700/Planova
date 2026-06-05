using Planova.Domain.Entities;

namespace Planova.Application.Repositories;

public interface IUserProfileRepository
{
    Task<UserPreferences?> GetAsync(CancellationToken ct = default);
    Task<UserPreferences> SaveAsync(UserPreferences preferences, CancellationToken ct = default);
}
