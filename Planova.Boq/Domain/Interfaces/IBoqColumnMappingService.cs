namespace Planova.Boq.Domain.Interfaces;

public interface IBoqColumnMappingService
{
    Task<ColumnMapping> DetectMappingAsync(string filePath, int worksheetIndex, CancellationToken ct);
    Task<MappingValidation> ValidateMappingAsync(ColumnMapping mapping, CancellationToken ct);
}

public record MappingValidation(bool IsValid, IReadOnlyList<string> Warnings);
