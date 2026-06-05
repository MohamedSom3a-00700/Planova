namespace Planova.Resource.Application.Dto;

public record ImportPreviewDto
{
    public string FileName { get; init; } = string.Empty;
    public int TotalRows { get; init; }
    public int ValidRows { get; init; }
    public int ErrorRows { get; init; }
    public List<ImportRowDto> Rows { get; init; } = [];
    public List<ImportDuplicateDto> Duplicates { get; init; } = [];
}

public record ImportRowDto
{
    public int RowNumber { get; init; }
    public string? Name { get; init; }
    public string? ResourceType { get; init; }
    public string? Code { get; init; }
    public decimal? DefaultRate { get; init; }
    public string? UnitOfMeasure { get; init; }
    public List<string> ValidationErrors { get; init; } = [];
    public bool IsValid { get; init; }
}

public record ImportDuplicateDto
{
    public int RowNumber { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string ExistingResourceCode { get; init; } = string.Empty;
    public string ExistingResourceName { get; init; } = string.Empty;
}

public record ImportRequest
{
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public int? ProjectId { get; init; }
    public ImportDuplicateHandling DuplicateHandling { get; init; }
    public List<int>? SelectedRowNumbers { get; init; }
}

public enum ImportDuplicateHandling
{
    Skip,
    Overwrite,
    Rename
}

public record ImportResultDto
{
    public int TotalProcessed { get; init; }
    public int SuccessCount { get; init; }
    public int SkippedCount { get; init; }
    public int ErrorCount { get; init; }
    public List<string> Errors { get; init; } = [];
    public List<string> Warnings { get; init; } = [];
}
