namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsCodeGenerationService
{
    Task RegenerateCodesAsync(Guid wbsId, CancellationToken ct);
    Task<bool> ValidateCodesAsync(Guid wbsId, CancellationToken ct);
}
