namespace Planova.Boq.Application.Dto;

public record ExportOptions(
    bool IncludeHeaders,
    bool IncludeSubtotals,
    bool IncludeGrandTotal,
    string? SheetName,
    string OutputPath
);

public record ExportResult(
    string OutputPath,
    long FileSize,
    int ItemCount,
    TimeSpan Duration,
    bool Success
);
