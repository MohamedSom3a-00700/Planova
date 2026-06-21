using System.Text.Json;
using ClosedXML.Excel;
using Planova.Boq.Application.Dto;
using Planova.Boq.Application.Models;
using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Application.Services;

public sealed class MultiSheetBoqImportService : IMultiSheetBoqImportService
{
    private static readonly string[] NonBoqSheetPatterns =
        ["cover", "summary", "index", "instructions", "notes", "calculation", "title", "contents", "toc", "disclaimer"];

    private static readonly string[] BoqKeywordPatterns =
        ["boq", "bill of quantities", "billofquantities", "schedule of quantities", "scheduleofquantities",
         "pricing", "rate", "quantity", "estimate", "cost estimate", "costestimate"];

    private readonly IBoqRepository _boqRepository;
    private readonly IBoqItemRepository _itemRepository;
    private readonly IBoqColumnMappingService _columnMappingService;
    private readonly IExcelRowReader _excelRowReader;
    private readonly ITreeBuilder _treeBuilder;

    public MultiSheetBoqImportService(
        IBoqRepository boqRepository,
        IBoqItemRepository itemRepository,
        IBoqColumnMappingService columnMappingService,
        IExcelRowReader excelRowReader,
        ITreeBuilder treeBuilder)
    {
        _boqRepository = boqRepository;
        _itemRepository = itemRepository;
        _columnMappingService = columnMappingService;
        _excelRowReader = excelRowReader;
        _treeBuilder = treeBuilder;
    }

    public async Task<IReadOnlyList<WorksheetInfo>> ScanAsync(string filePath, CancellationToken ct)
    {
        if (StreamingExcelReader.ShouldStream(filePath))
            return await ScanStreamingAsync(filePath, ct);

        using var stream = File.OpenRead(filePath);
        using var workbook = new XLWorkbook(stream);

        var results = new List<WorksheetInfo>();
        var index = 0;

        foreach (var ws in workbook.Worksheets)
        {
            var name = ws.Name?.Trim() ?? string.Empty;
            var rowCount = ws.LastRowUsed()?.RowNumber() ?? 0;

            var isBoq = !IsNonBoqSheet(name);
            var confidence = 0m;

            if (isBoq)
            {
                confidence = CalculateBoqConfidence(ws);
                if (confidence < 0.2m && !name.Contains("boq", StringComparison.OrdinalIgnoreCase))
                    isBoq = false;
            }

            results.Add(new WorksheetInfo(name, index, rowCount, isBoq, confidence));
            index++;
        }

        return results.AsReadOnly();
    }

    private static async Task<IReadOnlyList<WorksheetInfo>> ScanStreamingAsync(string filePath, CancellationToken ct)
    {
        using var reader = await StreamingExcelReader.OpenAsync(filePath, ct);
        var results = new List<WorksheetInfo>();
        var index = 0;

        foreach (var sheetName in reader.SheetNames)
        {
            ct.ThrowIfCancellationRequested();
            var name = sheetName.Trim();
            var isBoq = !IsNonBoqSheet(name);
            var confidence = 0m;

            if (isBoq)
            {
                confidence = await StreamCalculateBoqConfidenceAsync(reader, index, ct);
                if (confidence < 0.2m && !name.Contains("boq", StringComparison.OrdinalIgnoreCase))
                    isBoq = false;
            }

            var rowCount = await StreamCountRowsAsync(reader, index, ct);
            results.Add(new WorksheetInfo(name, index, rowCount, isBoq, confidence));
            index++;
        }

        return results.AsReadOnly();
    }

    public async Task<BoqScanPreview> PreviewAsync(
        string filePath,
        Guid projectId,
        ImportMode mode,
        IReadOnlyList<WorksheetSelection> selections,
        CancellationToken ct)
    {
        var scanResults = await ScanAsync(filePath, ct);
        var previews = new List<WorksheetPreview>();

        foreach (var selection in selections)
        {
            if (selection.Action == ImportAction.Skip)
                continue;

            var scan = scanResults.FirstOrDefault(s => s.Index == selection.WorksheetIndex);
            if (scan is null) continue;

            var mapping = selection.OverrideMapping
                ?? await _columnMappingService.DetectMappingAsync(filePath, selection.WorksheetIndex, ct);

            var matchedColumns = 0;
            if (mapping.CodeColumn.HasValue) matchedColumns++;
            if (mapping.DescriptionColumn.HasValue) matchedColumns++;
            if (mapping.UnitColumn.HasValue) matchedColumns++;
            if (mapping.QuantityColumn.HasValue) matchedColumns++;
            if (mapping.RateColumn.HasValue) matchedColumns++;
            if (mapping.AmountColumn.HasValue) matchedColumns++;

            previews.Add(new WorksheetPreview(
                selection.WorksheetIndex,
                scan.Name,
                scan.RowCount,
                matchedColumns
            ));
        }

        return new BoqScanPreview(
            scanResults.Count,
            previews.Count,
            previews.Sum(p => p.RowCount),
            previews.AsReadOnly()
        );
    }

    public async Task<BoqImportSummary> ImportAsync(
        string filePath,
        Guid projectId,
        ImportMode mode,
        IReadOnlyList<WorksheetSelection> selections,
        int userId,
        CancellationToken ct)
    {
        var activeSelections = selections.Where(s => s.Action != ImportAction.Skip).ToList();
        var isStreaming = StreamingExcelReader.ShouldStream(filePath);

        var session = new BoqImportSession
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FileName = Path.GetFileName(filePath),
            FilePath = filePath,
            ImportMode = mode,
            ImportedAt = DateTime.UtcNow,
            ImportedByUserId = userId,
            Status = ImportStatus.Completed
        };

        switch (mode)
        {
            case ImportMode.Single:
                if (isStreaming)
                    await StreamImportSingleAsync(filePath, projectId, activeSelections, session, ct);
                else
                    await ImportSingleModeAsync(filePath, projectId, activeSelections, session, ct);
                break;
            case ImportMode.Multiple:
                if (isStreaming)
                    await StreamImportMultipleAsync(filePath, projectId, activeSelections, session, ct);
                else
                    await ImportMultipleModeAsync(filePath, projectId, activeSelections, session, ct);
                break;
            case ImportMode.Merge:
                if (isStreaming)
                    await ImportMergeModeWithStreamingAsync(filePath, projectId, activeSelections, session, ct);
                else
                    await ImportMergeModeAsync(filePath, projectId, activeSelections, session, ct);
                break;
        }

        return new BoqImportSummary(
            session.Id,
            session.TotalSheetsImported,
            session.TotalSectionsCreated,
            session.TotalItemsImported,
            session.TotalAmount
        );
    }

    private async Task StreamImportSingleAsync(
        string filePath, Guid projectId,
        List<WorksheetSelection> activeSelections,
        BoqImportSession session, CancellationToken ct)
    {
        var sel = activeSelections.FirstOrDefault();
        if (sel is null) return;

        var (boq, items, _) = await StreamImportWorksheetAsync(filePath, projectId, sel, session.Id, ct);
        var sectionCount = items.Count(i => i.ItemType == ItemType.Section);
        session.TotalSheetsImported = 1;
        session.TotalSectionsCreated = sectionCount;
        session.TotalItemsImported = items.Count;
        session.TotalAmount = boq.TotalAmount;
    }

    private async Task StreamImportMultipleAsync(
        string filePath, Guid projectId,
        List<WorksheetSelection> activeSelections,
        BoqImportSession session, CancellationToken ct)
    {
        var allItems = new List<Domain.Entities.BoqItem>();
        var sections = 0;
        foreach (var sel in activeSelections)
        {
            var (boq, items, _) = await StreamImportWorksheetAsync(filePath, projectId, sel, session.Id, ct);
            allItems.AddRange(items);
            sections += items.Count(i => i.ItemType == ItemType.Section);
        }
        session.TotalSheetsImported = activeSelections.Count;
        session.TotalSectionsCreated = sections;
        session.TotalItemsImported = allItems.Count;
        session.TotalAmount = allItems.Sum(i => i.Amount);
    }

    private async Task ImportSingleModeAsync(
        string filePath, Guid projectId,
        List<WorksheetSelection> activeSelections,
        BoqImportSession session, CancellationToken ct)
    {
        var sel = activeSelections.FirstOrDefault();
        if (sel is null) return;

        var (boq, items, _) = await ImportWorksheetAsync(filePath, projectId, sel, session.Id, ct);
        var sectionCount = items.Count(i => i.ItemType == ItemType.Section);
        session.TotalSheetsImported = 1;
        session.TotalSectionsCreated = sectionCount;
        session.TotalItemsImported = items.Count;
        session.TotalAmount = boq.TotalAmount;
    }

    private async Task ImportMultipleModeAsync(
        string filePath, Guid projectId,
        List<WorksheetSelection> activeSelections,
        BoqImportSession session, CancellationToken ct)
    {
        var allItems = new List<Domain.Entities.BoqItem>();
        var sections = 0;
        foreach (var sel in activeSelections)
        {
            var (boq, items, _) = await ImportWorksheetAsync(filePath, projectId, sel, session.Id, ct);
            allItems.AddRange(items);
            sections += items.Count(i => i.ItemType == ItemType.Section);
        }
        session.TotalSheetsImported = activeSelections.Count;
        session.TotalSectionsCreated = sections;
        session.TotalItemsImported = allItems.Count;
        session.TotalAmount = allItems.Sum(i => i.Amount);
    }

    private async Task ImportMergeModeAsync(
        string filePath, Guid projectId,
        List<WorksheetSelection> activeSelections,
        BoqImportSession session, CancellationToken ct)
    {
        var rootBoq = new Domain.Entities.Boq
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = Path.GetFileNameWithoutExtension(filePath),
            Currency = "USD",
            Status = BoqStatus.Draft,
            ImportSource = "Excel",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            CreatedBy = "system",
            ModifiedBy = "system",
            ImportedAt = DateTime.UtcNow
        };

        await _boqRepository.AddAsync(rootBoq, ct);
        var sortOrder = 0;
        var allItems = new List<Domain.Entities.BoqItem>();
        var scanResults = await ScanAsync(filePath, ct);

        foreach (var sel in activeSelections)
        {
            var mapping = sel.OverrideMapping
                ?? await _columnMappingService.DetectMappingAsync(filePath, sel.WorksheetIndex, ct);

            var scan = scanResults.FirstOrDefault(s => s.Index == sel.WorksheetIndex);
            var sheetName = scan?.Name ?? $"Sheet{sel.WorksheetIndex + 1}";

            var importRows = await _excelRowReader.ReadAsync(filePath, sheetName, ct);
            var strategy = _treeBuilder.DetectStrategy(importRows);
            var items = _treeBuilder.BuildTree(importRows, strategy);

            sortOrder++;
            var sectionAmount = items.Sum(i => i.Amount);
            var sectionItem = new Domain.Entities.BoqItem
            {
                Id = Guid.NewGuid(),
                BoqId = rootBoq.Id,
                Code = sheetName,
                Description = $"Items from {sheetName}",
                Unit = "LS",
                Quantity = 1,
                Rate = sectionAmount,
                Amount = sectionAmount,
                ItemType = ItemType.Section,
                Level = 0,
                SortOrder = sortOrder,
                IsActive = true
            };

            foreach (var item in items)
            {
                item.BoqId = rootBoq.Id;
                item.ParentId = sectionItem.Id;
                item.Level = 1;
                sortOrder++;
                item.SortOrder = sortOrder;
            }

            allItems.Add(sectionItem);
            allItems.AddRange(items);

            var mappingJson = JsonSerializer.Serialize(mapping);
            session.WorksheetMappings.Add(new BoqWorksheetMapping
            {
                Id = Guid.NewGuid(),
                ImportSessionId = session.Id,
                WorksheetName = sheetName,
                SortOrder = sel.WorksheetIndex,
                MatchConfidence = mapping.Confidence,
                ColumnMappings = mappingJson,
                RowsImported = items.Count,
                SectionAmount = sectionAmount
            });
        }

        var (deduplicated, _) = DeduplicateItems(allItems);
        await _itemRepository.AddRangeAsync(deduplicated, ct);

        rootBoq.TotalAmount = deduplicated.Sum(i => i.Amount);
        rootBoq.Items = deduplicated;
        await _boqRepository.UpdateAsync(rootBoq, ct);

        session.TotalSheetsImported = activeSelections.Count;
        session.TotalSectionsCreated = activeSelections.Count;
        session.TotalItemsImported = deduplicated.Count;
        session.TotalAmount = rootBoq.TotalAmount;
    }

    private async Task<(Domain.Entities.Boq Boq, List<Domain.Entities.BoqItem> Items, int Skipped)> ImportWorksheetAsync(
        string filePath, Guid projectId, WorksheetSelection selection, Guid sessionId, CancellationToken ct)
    {
        var mapping = selection.OverrideMapping
            ?? await _columnMappingService.DetectMappingAsync(filePath, selection.WorksheetIndex, ct);

        var scanResults = await ScanAsync(filePath, ct);
        var scan = scanResults.FirstOrDefault(s => s.Index == selection.WorksheetIndex);
        var sheetName = scan?.Name ?? $"Sheet{selection.WorksheetIndex + 1}";

        var importRows = await _excelRowReader.ReadAsync(filePath, sheetName, ct);
        var strategy = _treeBuilder.DetectStrategy(importRows);
        var items = _treeBuilder.BuildTree(importRows, strategy);

        var boq = new Domain.Entities.Boq
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = sheetName,
            Currency = "USD",
            Status = BoqStatus.Draft,
            ImportSource = "Excel",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            CreatedBy = "system",
            ModifiedBy = "system",
            ImportedAt = DateTime.UtcNow
        };

        foreach (var item in items)
            item.BoqId = boq.Id;

        var (deduplicated, skipped) = DeduplicateItems(items);
        boq.Items = deduplicated;
        boq.TotalAmount = deduplicated.Sum(i => i.Amount);

        await _boqRepository.AddAsync(boq, ct);
        await _itemRepository.AddRangeAsync(deduplicated, ct);

        return (boq, deduplicated, skipped);
    }

    private static (List<Domain.Entities.BoqItem> Deduplicated, int Skipped) DeduplicateItems(IEnumerable<Domain.Entities.BoqItem> items)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<Domain.Entities.BoqItem>();
        var skipped = 0;
        var unnamedIndex = 0;
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Code))
            {
                item.Code = $"_ROW_{unnamedIndex++}";
                result.Add(item);
            }
            else
            {
                var key = item.Code.Trim();
                if (seen.Add(key))
                    result.Add(item);
                else
                    skipped++;
            }
        }
        return (result, skipped);
    }

    private async Task ImportMergeModeWithStreamingAsync(
        string filePath, Guid projectId,
        List<WorksheetSelection> activeSelections,
        BoqImportSession session, CancellationToken ct)
    {
        var rootBoq = new Domain.Entities.Boq
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = Path.GetFileNameWithoutExtension(filePath),
            Currency = "USD",
            Status = BoqStatus.Draft,
            ImportSource = "Excel",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            CreatedBy = "system",
            ModifiedBy = "system",
            ImportedAt = DateTime.UtcNow
        };

        await _boqRepository.AddAsync(rootBoq, ct);
        var sortOrder = 0;
        var allItems = new List<Domain.Entities.BoqItem>();

        using var reader = await StreamingExcelReader.OpenAsync(filePath, ct);
        var scanResults = await ScanStreamingAsync(filePath, ct);

        foreach (var sel in activeSelections)
        {
            var mapping = sel.OverrideMapping
                ?? await _columnMappingService.DetectMappingAsync(filePath, sel.WorksheetIndex, ct);

            var scan = scanResults.FirstOrDefault(s => s.Index == sel.WorksheetIndex);
            var sheetName = scan?.Name ?? $"Sheet{sel.WorksheetIndex + 1}";

            var importRows = new List<ImportRow>();
            await foreach (var row in reader.ReadSheetAsync(sel.WorksheetIndex, ct))
                importRows.Add(row);

            var strategy = _treeBuilder.DetectStrategy(importRows);
            var items = _treeBuilder.BuildTree(importRows, strategy);

            sortOrder++;
            var sectionAmount = items.Sum(i => i.Amount);
            var sectionItem = new Domain.Entities.BoqItem
            {
                Id = Guid.NewGuid(),
                BoqId = rootBoq.Id,
                Code = sheetName,
                Description = $"Items from {sheetName}",
                Unit = "LS",
                Quantity = 1,
                Rate = sectionAmount,
                Amount = sectionAmount,
                ItemType = ItemType.Section,
                Level = 0,
                SortOrder = sortOrder,
                IsActive = true
            };

            foreach (var item in items)
            {
                item.BoqId = rootBoq.Id;
                item.ParentId = sectionItem.Id;
                item.Level = 1;
                sortOrder++;
                item.SortOrder = sortOrder;
            }

            allItems.Add(sectionItem);
            allItems.AddRange(items);

            var mappingJson = JsonSerializer.Serialize(mapping);
            session.WorksheetMappings.Add(new BoqWorksheetMapping
            {
                Id = Guid.NewGuid(),
                ImportSessionId = session.Id,
                WorksheetName = sheetName,
                SortOrder = sel.WorksheetIndex,
                MatchConfidence = mapping.Confidence,
                ColumnMappings = mappingJson,
                RowsImported = items.Count,
                SectionAmount = sectionAmount
            });
        }

        var (deduplicated, _) = DeduplicateItems(allItems);
        await _itemRepository.AddRangeAsync(deduplicated, ct);

        rootBoq.TotalAmount = deduplicated.Sum(i => i.Amount);
        rootBoq.Items = deduplicated;
        await _boqRepository.UpdateAsync(rootBoq, ct);

        session.TotalSheetsImported = activeSelections.Count;
        session.TotalSectionsCreated = activeSelections.Count;
        session.TotalItemsImported = deduplicated.Count;
        session.TotalAmount = rootBoq.TotalAmount;
    }

    private async Task<(Domain.Entities.Boq, List<Domain.Entities.BoqItem>, int)> StreamImportWorksheetAsync(
        string filePath, Guid projectId, WorksheetSelection selection, Guid sessionId, CancellationToken ct)
    {
        var mapping = selection.OverrideMapping
            ?? await _columnMappingService.DetectMappingAsync(filePath, selection.WorksheetIndex, ct);

        var sheetName = string.Empty;
        using var reader = await StreamingExcelReader.OpenAsync(filePath, ct);

        if (selection.WorksheetIndex < reader.SheetNames.Count)
            sheetName = reader.SheetNames[selection.WorksheetIndex];

        var importRows = new List<ImportRow>();
        await foreach (var row in reader.ReadSheetAsync(selection.WorksheetIndex, ct))
            importRows.Add(row);

        var strategy = _treeBuilder.DetectStrategy(importRows);
        var items = _treeBuilder.BuildTree(importRows, strategy);

        var boq = new Domain.Entities.Boq
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = string.IsNullOrEmpty(sheetName) ? $"Sheet{selection.WorksheetIndex + 1}" : sheetName,
            Currency = "USD",
            Status = BoqStatus.Draft,
            ImportSource = "Excel",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            CreatedBy = "system",
            ModifiedBy = "system",
            ImportedAt = DateTime.UtcNow
        };

        foreach (var item in items)
            item.BoqId = boq.Id;

        var (deduplicated, skipped) = DeduplicateItems(items);
        boq.Items = deduplicated;
        boq.TotalAmount = deduplicated.Sum(i => i.Amount);

        await _boqRepository.AddAsync(boq, ct);
        await _itemRepository.AddRangeAsync(deduplicated, ct);

        return (boq, deduplicated, skipped);
    }

    private static async Task<decimal> StreamCalculateBoqConfidenceAsync(StreamingExcelReader reader, int sheetIndex, CancellationToken ct)
    {
        var headerCols = 0;
        var matches = 0;

        await foreach (var row in reader.ReadSheetAsync(sheetIndex, ct))
        {
            if (!string.IsNullOrWhiteSpace(row.Code)) { headerCols++; if (BoqKeywordPatterns.Any(p => row.Code.Contains(p, StringComparison.OrdinalIgnoreCase))) matches++; }
            if (!string.IsNullOrWhiteSpace(row.Description)) { headerCols++; if (BoqKeywordPatterns.Any(p => row.Description.Contains(p, StringComparison.OrdinalIgnoreCase))) matches++; }
            if (!string.IsNullOrWhiteSpace(row.Unit)) headerCols++;
            if (row.Quantity > 0) { headerCols++; if (BoqKeywordPatterns.Any(p => "quantity".Contains(p, StringComparison.OrdinalIgnoreCase))) matches++; }
            if (row.Rate > 0) { headerCols++; if (BoqKeywordPatterns.Any(p => "rate".Contains(p, StringComparison.OrdinalIgnoreCase))) matches++; }
            break;
        }

        return headerCols > 0 ? (decimal)matches / headerCols : 0;
    }

    private static async Task<int> StreamCountRowsAsync(StreamingExcelReader reader, int sheetIndex, CancellationToken ct)
    {
        var count = 0;
        await foreach (var _ in reader.ReadSheetAsync(sheetIndex, ct))
            count++;
        return count;
    }

    private static bool IsNonBoqSheet(string name)
    {
        return NonBoqSheetPatterns.Any(p => name.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private static decimal CalculateBoqConfidence(IXLWorksheet ws)
    {
        var headerRow = ws.Row(1);
        var lastCell = headerRow.LastCellUsed();
        if (lastCell is null) return 0;

        var matches = 0;
        var checkedCols = 0;

        for (int col = 1; col <= lastCell.Address.ColumnNumber; col++)
        {
            var value = headerRow.Cell(col).GetString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(value)) continue;

            checkedCols++;
            if (BoqKeywordPatterns.Any(p => value.Contains(p, StringComparison.OrdinalIgnoreCase)))
                matches++;
        }

        return checkedCols > 0 ? (decimal)matches / checkedCols : 0;
    }
}
