using Planova.Activity.Application.Dto;

namespace Planova.Activity.Domain.Interfaces;

public interface IActivityBankService
{
    Task<ActivityBankDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ActivityBankDto>> BrowseAsync(string? category = null, string? search = null, CancellationToken ct = default);
    Task<List<string>> GetCategoriesAsync(CancellationToken ct = default);
    Task<ActivityBankDto> CreateCustomAsync(CreateBankEntryRequest request, CancellationToken ct = default);
    Task<ActivityBankDto> UpdateCustomAsync(UpdateBankEntryRequest request, CancellationToken ct = default);
    Task DeleteCustomAsync(Guid id, CancellationToken ct = default);
    Task SeedIfEmptyAsync(CancellationToken ct = default);
    Task<ActivityBankDto> SaveActivitiesAsBankEntryAsync(List<Guid> activityIds, string category, string name, string? description = null, CancellationToken ct = default);
}
