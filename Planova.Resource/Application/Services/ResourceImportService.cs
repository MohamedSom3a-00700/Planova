using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Enums;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Resource.Application.Services;

public class ResourceImportService : IResourceImportService
{
    private readonly IResourceRepository _repository;
    private readonly IResourceService _resourceService;

    public ResourceImportService(IResourceRepository repository, IResourceService resourceService)
    {
        _repository = repository;
        _resourceService = resourceService;
    }

    public async Task<ImportPreviewDto> PreviewImportAsync(Stream fileStream, string fileName, int? projectId, CancellationToken ct = default)
    {
        using var reader = new StreamReader(fileStream);
        var content = await reader.ReadToEndAsync(ct);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var rows = new List<ImportRowDto>();
        var duplicates = new List<ImportDuplicateDto>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (i == 0 && line.StartsWith("Name", StringComparison.OrdinalIgnoreCase))
                continue;

            var parts = line.Split(',');
            var row = new ImportRowDto
            {
                RowNumber = i + 1,
                Name = parts.Length > 0 ? parts[0].Trim() : null,
                ResourceType = parts.Length > 1 ? parts[1].Trim() : null,
                Code = parts.Length > 2 ? parts[2].Trim() : null,
                DefaultRate = parts.Length > 3 && decimal.TryParse(parts[3].Trim(), out var rate) ? rate : null,
                UnitOfMeasure = parts.Length > 4 ? parts[4].Trim() : null
            };

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(row.Name))
                errors.Add("Name is required");
            if (string.IsNullOrWhiteSpace(row.ResourceType))
                errors.Add("ResourceType is required");
            else if (!Enum.TryParse<ResourceType>(row.ResourceType, true, out _))
                errors.Add($"Invalid ResourceType: {row.ResourceType}");

            row = row with
            {
                ValidationErrors = errors,
                IsValid = errors.Count == 0
            };

            if (row.IsValid && row.Name is not null)
            {
                var hasDuplicate = await _repository.HasDuplicateNameAsync(row.Name, ResourceScope.Global, null, null, ct);
                if (hasDuplicate)
                {
                    var existing = await _repository.SearchAsync(row.Name!, null, null, null, ct);
                    var first = existing.FirstOrDefault();
                    if (first is not null)
                    {
                        duplicates.Add(new ImportDuplicateDto
                        {
                            RowNumber = row.RowNumber,
                            Code = row.Code,
                            Name = row.Name,
                            ExistingResourceCode = first.Code,
                            ExistingResourceName = first.Name
                        });
                    }
                }
            }

            rows.Add(row);
        }

        return new ImportPreviewDto
        {
            FileName = fileName,
            TotalRows = rows.Count,
            ValidRows = rows.Count(r => r.IsValid),
            ErrorRows = rows.Count(r => !r.IsValid),
            Rows = rows,
            Duplicates = duplicates
        };
    }

    public async Task<ImportResultDto> ExecuteImportAsync(ImportRequest request, CancellationToken ct = default)
    {
        var preview = await PreviewImportAsync(request.FileStream, request.FileName, request.ProjectId, ct);
        var validRows = request.SelectedRowNumbers is not null
            ? preview.Rows.Where(r => request.SelectedRowNumbers.Contains(r.RowNumber)).ToList()
            : preview.Rows.Where(r => r.IsValid).ToList();

        int success = 0, skipped = 0, errors = 0;
        var errorList = new List<string>();
        var warnings = new List<string>();

        foreach (var row in validRows)
        {
            try
            {
                var isDuplicate = preview.Duplicates.Any(d => d.RowNumber == row.RowNumber);

                if (isDuplicate && request.DuplicateHandling == ImportDuplicateHandling.Skip)
                {
                    skipped++;
                    continue;
                }

                if (!Enum.TryParse<ResourceType>(row.ResourceType, true, out var resourceType))
                {
                    errors++;
                    errorList.Add($"Row {row.RowNumber}: Invalid resource type '{row.ResourceType}'");
                    continue;
                }

                var createRequest = new CreateResourceRequest
                {
                    Name = row.Name ?? $"Imported-{Guid.NewGuid():N}",
                    ResourceType = resourceType,
                    Scope = request.ProjectId.HasValue ? ResourceScope.Project : ResourceScope.Global,
                    ProjectId = request.ProjectId,
                    DefaultRate = row.DefaultRate ?? 0,
                    UnitOfMeasure = row.UnitOfMeasure ?? "hr"
                };

                if (isDuplicate && request.DuplicateHandling == ImportDuplicateHandling.Rename)
                {
                    createRequest = createRequest with
                    {
                        Name = $"{row.Name}-imported"
                    };
                    warnings.Add($"Row {row.RowNumber}: Renamed to '{createRequest.Name}'");
                }

                await _resourceService.CreateAsync(createRequest, ct);
                success++;
            }
            catch (Exception ex)
            {
                errors++;
                errorList.Add($"Row {row.RowNumber}: {ex.Message}");
            }
        }

        return new ImportResultDto
        {
            TotalProcessed = validRows.Count,
            SuccessCount = success,
            SkippedCount = skipped,
            ErrorCount = errors,
            Errors = errorList,
            Warnings = warnings
        };
    }
}
