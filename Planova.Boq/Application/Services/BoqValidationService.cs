using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Application.Services;

public class BoqValidationService : IBoqValidationService
{
    private readonly IBoqItemRepository _itemRepository;
    private static readonly HashSet<string> KnownUnits = ["EA", "LS", "M2", "M3", "HR", "DAY", "WEEK", "MONTH", "KG", "TON", "M", "KM", "NO"];

    public BoqValidationService(IBoqItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task<ValidationResult> ValidateStructureAsync(Guid boqId, CancellationToken ct)
    {
        var errors = new List<ValidationIssue>();
        var warnings = new List<ValidationIssue>();

        errors.AddRange(await DetectDuplicatesAsync(boqId, ct));
        errors.AddRange(await DetectCircularReferencesAsync(boqId, ct));
        errors.AddRange(await DetectOrphansAsync(boqId, ct));

        var items = await _itemRepository.GetByBoqIdAsync(boqId, ct);
        foreach (var item in items)
        {
            var result = await ValidateItemAsync(item, ct);
            errors.AddRange(result.Errors);
            warnings.AddRange(result.Warnings);
        }

        return new ValidationResult(errors.Count == 0, errors, warnings);
    }

    public async Task<ValidationResult> ValidateImportAsync(IReadOnlyList<ImportRow> rows, CancellationToken ct)
    {
        var errors = new List<ValidationIssue>();
        var warnings = new List<ValidationIssue>();

        var codes = new HashSet<string>();
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Code))
            {
                errors.Add(new ValidationIssue(null, row.Code, "Code", IssueType.Error, "Code is required", null));
            }
            else if (!codes.Add(row.Code))
            {
                errors.Add(new ValidationIssue(null, row.Code, "Code", IssueType.Error, $"Duplicate code: {row.Code}", "Rename to a unique code"));
            }

            if (string.IsNullOrWhiteSpace(row.Unit) || !KnownUnits.Contains(row.Unit.ToUpperInvariant()))
            {
                warnings.Add(new ValidationIssue(null, row.Code, "Unit", IssueType.Warning,
                    $"Unrecognized unit: '{row.Unit}'", $"Use one of: {string.Join(", ", KnownUnits)}"));
            }

            if (row.Quantity < 0)
                errors.Add(new ValidationIssue(null, row.Code, "Quantity", IssueType.Error, "Quantity must be >= 0", null));

            if (row.Rate < 0)
                errors.Add(new ValidationIssue(null, row.Code, "Rate", IssueType.Error, "Rate must be >= 0", null));
        }

        return new ValidationResult(errors.Count == 0, errors, warnings);
    }

    public Task<ValidationResult> ValidateItemAsync(BoqItem item, CancellationToken ct)
    {
        var errors = new List<ValidationIssue>();
        var warnings = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(item.Code))
            errors.Add(new ValidationIssue(item.Id, item.Code, "Code", IssueType.Error, "Code is required", null));

        if (item.Code.Length > 50)
            errors.Add(new ValidationIssue(item.Id, item.Code, "Code", IssueType.Error, "Code exceeds 50 characters", "Shorten the code"));

        if (string.IsNullOrWhiteSpace(item.Description))
            errors.Add(new ValidationIssue(item.Id, item.Code, "Description", IssueType.Error, "Description is required", null));

        if (item.Quantity < 0)
            errors.Add(new ValidationIssue(item.Id, item.Code, "Quantity", IssueType.Error, "Quantity must be >= 0", null));

        if (item.Rate < 0)
            errors.Add(new ValidationIssue(item.Id, item.Code, "Rate", IssueType.Error, "Rate must be >= 0", null));

        if (!string.IsNullOrWhiteSpace(item.Unit) && !KnownUnits.Contains(item.Unit.ToUpperInvariant()))
            warnings.Add(new ValidationIssue(item.Id, item.Code, "Unit", IssueType.Warning,
                $"Unrecognized unit: '{item.Unit}'", $"Use one of: {string.Join(", ", KnownUnits)}"));

        return Task.FromResult(new ValidationResult(errors.Count == 0, errors, warnings));
    }

    public async Task<IReadOnlyList<ValidationIssue>> DetectDuplicatesAsync(Guid boqId, CancellationToken ct)
    {
        var items = await _itemRepository.GetByBoqIdAsync(boqId, ct);
        var issues = new List<ValidationIssue>();
        var seen = new HashSet<string>();

        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(item.Code) && !seen.Add(item.Code))
            {
                issues.Add(new ValidationIssue(item.Id, item.Code, "Code", IssueType.Error,
                    $"Duplicate code '{item.Code}' in BOQ", "Rename to a unique code"));
            }
        }

        return issues;
    }

    public async Task<IReadOnlyList<ValidationIssue>> DetectCircularReferencesAsync(Guid boqId, CancellationToken ct)
    {
        var items = await _itemRepository.GetByBoqIdAsync(boqId, ct);
        var issues = new List<ValidationIssue>();
        var byId = items.ToDictionary(i => i.Id);

        foreach (var item in items)
        {
            if (item.ParentId == null) continue;
            if (HasCircularReference(item, byId, new HashSet<Guid>()))
            {
                issues.Add(new ValidationIssue(item.Id, item.Code, "ParentId", IssueType.Error,
                    $"Circular reference detected for item '{item.Code}'", "Change the parent to a valid ancestor"));
            }
        }

        return issues;
    }

    public async Task<IReadOnlyList<ValidationIssue>> DetectOrphansAsync(Guid boqId, CancellationToken ct)
    {
        var items = await _itemRepository.GetByBoqIdAsync(boqId, ct);
        var issues = new List<ValidationIssue>();
        var byId = items.ToDictionary(i => i.Id);

        foreach (var item in items.Where(i => i.ParentId.HasValue))
        {
            if (!byId.ContainsKey(item.ParentId!.Value))
            {
                issues.Add(new ValidationIssue(item.Id, item.Code, "ParentId", IssueType.Error,
                    $"Orphan item '{item.Code}' references non-existent parent", "Assign a valid parent or set as root-level item"));
            }
        }

        return issues;
    }

    private static bool HasCircularReference(BoqItem item, Dictionary<Guid, BoqItem> byId, HashSet<Guid> visited)
    {
        if (!visited.Add(item.Id)) return true;
        if (item.ParentId == null || !byId.TryGetValue(item.ParentId.Value, out var parent)) return false;
        return HasCircularReference(parent, byId, visited);
    }
}
