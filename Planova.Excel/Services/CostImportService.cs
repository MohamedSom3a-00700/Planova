using Planova.Activity.Domain.Interfaces;
using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Entities;
using Planova.Cost.Domain.Enums;
using Planova.Cost.Domain.Interfaces;
using Planova.Excel.Readers;
using Planova.Shared.Abstractions;

namespace Planova.Excel.Services;

public class CostImportService : ICostImportService
{
    private readonly IWorkbookReader _workbookReader;
    private readonly IActualCostRepository _actualCostRepository;
    private readonly IActivityService _activityService;
    private readonly ILoggingService _logger;

    public CostImportService(
        IWorkbookReader workbookReader,
        IActualCostRepository actualCostRepository,
        IActivityService activityService,
        ILoggingService logger)
    {
        _workbookReader = workbookReader;
        _actualCostRepository = actualCostRepository;
        _activityService = activityService;
        _logger = logger;
    }

    public async Task<ImportResultDto> ImportActualCostsAsync(
        Stream excelStream, int projectId, CancellationToken ct = default)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"cost_import_{Guid.NewGuid()}.xlsx");
        try
        {
            using (var fileStream = File.Create(tempPath))
            {
                await excelStream.CopyToAsync(fileStream, ct);
            }

            if (!_workbookReader.CanRead(tempPath))
            {
                return new ImportResultDto(0, 0, 0, 0, 0, false,
                    new List<string> { "Unsupported file format. Please provide an .xlsx file." },
                    new List<string>());
            }

            var workbook = await _workbookReader.OpenAsync(tempPath, ct);
            var sheetName = workbook.Worksheets.FirstOrDefault()?.Name;
            if (string.IsNullOrEmpty(sheetName))
            {
                return new ImportResultDto(0, 0, 0, 0, 0, false,
                    new List<string> { "No worksheets found in the workbook." },
                    new List<string>());
            }

            var records = await _workbookReader.ReadAllAsync(tempPath, sheetName, ct);
            var activities = await _activityService.GetByProjectAsync(projectId, null, ct);
            var activityLookup = activities.ToDictionary(a => a.Code, a => a.Id);

            var errors = new List<string>();
            var warnings = new List<string>();
            var matched = 0;
            var unmatched = 0;
            var created = 0;
            var updated = 0;

            foreach (var record in records)
            {
                try
                {
                    if (!record.TryGetValue("ActivityCode", out var codeObj) || codeObj == null)
                    {
                        unmatched++;
                        continue;
                    }
                    var activityCode = codeObj.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(activityCode))
                    {
                        unmatched++;
                        continue;
                    }

                    record.TryGetValue("Amount", out var amountObj);
                    var amountStr = amountObj?.ToString() ?? "0";

                    record.TryGetValue("Currency", out var currencyObj);
                    var currency = currencyObj?.ToString() ?? "USD";

                    record.TryGetValue("EntryDate", out var dateObj);
                    var dateStr = dateObj?.ToString();

                    if (!activityLookup.TryGetValue(activityCode, out var activityId))
                    {
                        unmatched++;
                        warnings.Add($"Activity code '{activityCode}' not found in project.");
                        continue;
                    }

                    if (!decimal.TryParse(amountStr, out var amount))
                    {
                        errors.Add($"Invalid amount '{amountStr}' for activity '{activityCode}'.");
                        unmatched++;
                        continue;
                    }

                    DateTime entryDate = DateTime.TryParse(dateStr, out var parsed) ? parsed : DateTime.UtcNow;
                    matched++;

                    var existing = await _actualCostRepository.GetByActivityIdAsync(activityId, ct);
                    if (existing != null)
                    {
                        existing.Amount = amount;
                        existing.Currency = currency;
                        existing.EntryDate = entryDate;
                        existing.Source = ActualCostSource.Imported;
                        existing.UpdatedAt = DateTime.UtcNow;
                        await _actualCostRepository.UpdateAsync(existing, ct);
                        updated++;
                    }
                    else
                    {
                        var entity = new ActualCost
                        {
                            Id = Guid.NewGuid(),
                            ProjectId = projectId,
                            ActivityId = activityId,
                            Amount = amount,
                            Currency = currency,
                            Source = ActualCostSource.Imported,
                            EntryDate = entryDate,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _actualCostRepository.AddAsync(entity, ct);
                        created++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing row: {ex.Message}");
                }
            }

            _logger.Info("Cost import completed: Total={Total}, Matched={Matched}, Unmatched={Unmatched}, Created={Created}, Updated={Updated}",
                records.Count, matched, unmatched, created, updated);

            return new ImportResultDto(
                records.Count, matched, unmatched, updated, created,
                errors.Count == 0, errors, warnings);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
