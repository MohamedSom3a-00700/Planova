using System.Diagnostics;
using Planova.Boq.Application.Dto;
using Planova.Boq.CsvReader;
using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Application.Services;

public class BoqImportService : IBoqImportService
{
    private readonly IBoqRepository _boqRepository;
    private readonly IBoqItemRepository _itemRepository;
    private readonly ITreeBuilder _treeBuilder;
    private readonly IBoqCsvReader _csvReader;
    private readonly IExcelRowReader _excelReader;

    public BoqImportService(
        IBoqRepository boqRepository,
        IBoqItemRepository itemRepository,
        ITreeBuilder treeBuilder,
        IBoqCsvReader csvReader,
        IExcelRowReader excelReader)
    {
        _boqRepository = boqRepository;
        _itemRepository = itemRepository;
        _treeBuilder = treeBuilder;
        _csvReader = csvReader;
        _excelReader = excelReader;
    }

    public async Task<BoqImportResult> ImportFromExcelAsync(
        Guid projectId, string filePath, Guid? mappingProfileId,
        IProgress<int> progress, CancellationToken ct)
    {
        return await ImportFromExcelAsync(projectId, filePath, null, mappingProfileId, progress, ct);
    }

    public async Task<BoqImportResult> ImportFromExcelAsync(
        Guid projectId, string filePath, string? sheetName, Guid? mappingProfileId,
        IProgress<int> progress, CancellationToken ct)
    {
        progress?.Report(0);

        var boq = new Domain.Entities.Boq
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = Path.GetFileNameWithoutExtension(filePath),
            Currency = "USD",
            Status = BoqStatus.Draft,
            ImportSource = "Excel",
            ImportedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            CreatedBy = "system",
            ModifiedBy = "system",
        };

        var stopwatch = Stopwatch.StartNew();
        var errors = new List<ValidationIssue>();

        progress?.Report(10);

        var importRows = await _excelReader.ReadAsync(filePath, sheetName, ct);
        if (importRows.Count == 0)
        {
            errors.Add(new ValidationIssue(null, "NO_DATA", "Data", IssueType.Error, "No data rows found in the workbook.", null));
            return new BoqImportResult(Guid.Empty, 0, 0, 0, errors, TimeSpan.Zero);
        }

        progress?.Report(30);
        var strategy = _treeBuilder.DetectStrategy(importRows);
        var items = _treeBuilder.BuildTree(importRows, strategy);
        var totalItems = importRows.Count;
        var totalSkipped = 0;

        var created = await _boqRepository.AddAsync(boq, ct);
        progress?.Report(60);

        foreach (var item in items)
        {
            item.BoqId = created.Id;
        }

        if (items.Count > 0)
        {
            created.Items = items.ToList();
            await _itemRepository.AddRangeAsync(items, ct);
        }

        progress?.Report(80);

        created.TotalAmount = items.Sum(i => i.Amount);
        await _boqRepository.UpdateAsync(created, ct);

        progress?.Report(100);
        stopwatch.Stop();

        return new BoqImportResult(created.Id, items.Count, 0, totalSkipped, errors, stopwatch.Elapsed);
    }

    public async Task<BoqImportResult> ImportFromCsvAsync(
        Guid projectId, string filePath, CsvImportOptions options,
        IProgress<int> progress, CancellationToken ct)
    {
        progress?.Report(0);

        var rows = await _csvReader.ReadAsync(filePath, options, ct);
        progress?.Report(30);

        var strategy = _treeBuilder.DetectStrategy(rows);
        var items = _treeBuilder.BuildTree(rows, strategy);
        progress?.Report(60);

        var boq = new Domain.Entities.Boq
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = Path.GetFileNameWithoutExtension(filePath),
            Currency = "USD",
            Status = BoqStatus.Draft,
            ImportSource = "CSV",
            ImportedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            CreatedBy = "system",
            ModifiedBy = "system",
        };

        var created = await _boqRepository.AddAsync(boq, ct);

        foreach (var item in items)
        {
            item.BoqId = created.Id;
        }

        if (items.Count > 0)
        {
            created.Items = items.ToList();
            await _itemRepository.AddRangeAsync(items, ct);
        }

        progress?.Report(100);

        return new BoqImportResult(created.Id, items.Count, 0, 0, [], TimeSpan.Zero);
    }

    public async Task<BoqImportPreview> PreviewImportAsync(
        IReadOnlyList<ImportRow> rows, TreeBuildStrategy strategy, CancellationToken ct)
    {
        var tree = _treeBuilder.BuildTree(rows, strategy);
        return new BoqImportPreview(tree, [], tree.Count);
    }
}
