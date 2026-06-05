using Planova.Application.Dto;

namespace Planova.Application.Services;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetProfileAsync(CancellationToken ct = default);
    Task<UserProfileDto> UpdateProfileAsync(UpdateUserProfileDto dto, CancellationToken ct = default);
}
