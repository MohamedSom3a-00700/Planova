using ClosedXML.Excel;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Application.Services;

public sealed class BoqColumnMappingService : IBoqColumnMappingService
{
    private static readonly Dictionary<string, string[]> ColumnPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Code"] = ["code", "item code", "itemcode", "ref", "reference", "id", "编号",
            "item no", "itemno", "item no.", "item#", "item number", "itemnumber",
            "line no", "lineno", "line no.", "line#", "line number",
            "serial", "serial no", "serialno", "serial no.", "sr no", "srno", "sr. no", "sno",
            "ref no", "refno", "ref#", "reference no", "referenceno",
            "بند", "رقم البند", "رقم", "الرقم", "no.", "num", "number",
            "code no", "codeno", "code number", "part no", "partno", "part number"],
        ["Description"] = ["description", "desc", "item description", "itemdescription", "name",
            "description of works", "descriptionofworks", "描述",
            "specification", "spec", "specifications", "particulars", "particular",
            "details", "detail", "work description", "workdescription",
            "item", "item name", "itemname", "title", "work item", "workitem",
            "scope of work", "scope", "scopework", "scopeofwork",
            "وصف", "تفاصيل", "بيان", "البند", "وصف الأعمال",
            "narrative", "text", "remark", "remarks", "note", "notes",
            "description of item", "descriptionofitem", "item detail", "itemdetail"],
        ["Unit"] = ["unit", "uom", "measurement unit", "measurementunit",
            "unit of measure", "unitofmeasure", "单位",
            "units", "unit of measurement", "unitofmeasurement",
            "measure", "measurement", "م", "م2", "m3", "kg", "ton", "ltr",
            "وحدة", "وحدة القياس"],
        ["Quantity"] = ["quantity", "qty", "qty.", "amount qty", "quantity", "数", "数量",
            "quantities", "volume", "nos", "no.", "no of", "number of",
            "count", "الكمية", "عدد", "qty no", "qtyno",
            "estimated quantity", "estimatedquantity", "budgeted quantity", "budgetedquantity",
            "quant", "quant.", "qty qty"],
        ["Rate"] = ["rate", "unit rate", "unitrate", "price", "unit price", "unitprice", "单价", "费率",
            "rates", "单价", "unit cost", "unitcost", "cost per unit", "costperunit",
            "price per unit", "priceperunit", "rate per unit", "rateperunit",
            "السعر", "سعر الوحدة", "معدل", "rate/unit",
            "bid rate", "bidrate", "tender rate", "tenderrate", "offered rate", "offeredrate"],
        ["Amount"] = ["amount", "total", "total amount", "totalamount", "cost",
            "total cost", "totalcost", "合计", "总价",
            "line total", "linetotal", "line amount", "lineamount",
            "extended amount", "extendedamount", "extended price", "extendedprice",
            "total price", "totalprice", "subtotal", "sum", "value",
            "الإجمالي", "المبلغ", "المجموع", "total value", "totalvalue",
            "grand total", "grandtotal", "net amount", "netamount",
            "bid amount", "bidamount", "tender amount", "tenderamount"]
    };

    public Task<ColumnMapping> DetectMappingAsync(string filePath, int worksheetIndex, CancellationToken ct)
    {
        using var stream = File.OpenRead(filePath);
        using var workbook = new XLWorkbook(stream);

        var ws = workbook.Worksheet(worksheetIndex + 1);
        if (ws is null)
            throw new InvalidOperationException($"Worksheet index {worksheetIndex} not found.");

        var headerRow = ws.Row(1);
        var lastCell = headerRow.LastCellUsed();
        var lastCol = lastCell?.Address.ColumnNumber ?? 1;

        int? codeCol = null, descCol = null, unitCol = null, qtyCol = null, rateCol = null, amountCol = null;
        var matchedCount = 0;

        for (int col = 1; col <= lastCol; col++)
        {
            var cellValue = headerRow.Cell(col).GetString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cellValue)) continue;

            var matched = false;
            foreach (var kvp in ColumnPatterns)
            {
                if (kvp.Value.Any(p => cellValue.Contains(p, StringComparison.OrdinalIgnoreCase)))
                {
                    var zeroBased = col - 1;
                    switch (kvp.Key)
                    {
                        case "Code": codeCol ??= zeroBased; matched = true; break;
                        case "Description": descCol ??= zeroBased; matched = true; break;
                        case "Unit": unitCol ??= zeroBased; matched = true; break;
                        case "Quantity": qtyCol ??= zeroBased; matched = true; break;
                        case "Rate": rateCol ??= zeroBased; matched = true; break;
                        case "Amount": amountCol ??= zeroBased; matched = true; break;
                    }
                    break;
                }
            }

            if (matched) matchedCount++;
        }

        var confidence = lastCol > 0 ? (decimal)matchedCount / Math.Min(lastCol, 6) : 0;
        confidence = Math.Min(confidence, 1m);

        return Task.FromResult(new ColumnMapping(codeCol, descCol, unitCol, qtyCol, rateCol, amountCol, confidence));
    }

    public Task<MappingValidation> ValidateMappingAsync(ColumnMapping mapping, CancellationToken ct)
    {
        var warnings = new List<string>();

        if (mapping.CodeColumn is null)
            warnings.Add("Code column not detected — BOQ items may not have unique identifiers.");
        if (mapping.DescriptionColumn is null)
            warnings.Add("Description column not detected — item descriptions will be empty.");
        if (mapping.QuantityColumn is null)
            warnings.Add("Quantity column not detected — quantities will be zero.");
        if (mapping.RateColumn is null)
            warnings.Add("Rate column not detected — rates will be zero.");
        if (mapping.AmountColumn is null)
            warnings.Add("Amount column not detected — amounts will be zero.");
        if (mapping.Confidence < 0.5m)
            warnings.Add("Low mapping confidence — consider reviewing column assignments.");

        return Task.FromResult(new MappingValidation(warnings.Count == 0, warnings.AsReadOnly()));
    }
}
