namespace Planova.Boq.Application.Dto;

public record ValidationResult(
    bool IsValid,
    IReadOnlyList<ValidationIssue> Errors,
    IReadOnlyList<ValidationIssue> Warnings
);
