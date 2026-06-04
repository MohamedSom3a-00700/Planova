using Planova.Boq.Domain.Entities;

namespace Planova.Boq.Application.Dto;

public record BoqImportResult(
    Guid BoqId,
    int ItemsImported,
    int ItemsUpdated,
    int ItemsSkipped,
    IReadOnlyList<ValidationIssue> Errors,
    TimeSpan Duration
);

public record BoqImportPreview(
    IReadOnlyList<BoqItem> Tree,
    IReadOnlyList<ValidationIssue> Warnings,
    int TotalItems
);

public record ValidationIssue(
    Guid? ItemId,
    string Code,
    string Field,
    IssueType Type,
    string Message,
    string? SuggestedFix
);

public enum IssueType { Error, Warning }
