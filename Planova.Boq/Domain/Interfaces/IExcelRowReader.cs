using Planova.Boq.Application.Dto;

namespace Planova.Boq.Domain.Interfaces;

public interface IExcelRowReader
{
    Task<IReadOnlyList<string>> GetWorksheetsAsync(string filePath, CancellationToken ct);
    Task<IReadOnlyList<ImportRow>> ReadAsync(string filePath, CancellationToken ct);
    Task<IReadOnlyList<ImportRow>> ReadAsync(string filePath, string? sheetName, CancellationToken ct);
}
