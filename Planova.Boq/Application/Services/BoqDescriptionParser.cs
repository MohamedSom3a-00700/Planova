using Planova.Boq.Application.Dto;

namespace Planova.Boq.Application.Services;

public interface IBoqDescriptionParser
{
    ParsedClassification ParseFromCode(string code);
    ParsedClassification ParseFromDescription(string description);
    ImportRow EnrichImportRow(ImportRow row, string? classificationColumn, string? divisionColumn, IReadOnlyDictionary<string, object> rawValues);
}

public sealed record ParsedClassification(
    string? DivisionCode,
    string? DivisionName,
    string? ClassificationCode,
    string? ClassificationName,
    string? CleanedDescription
);

public sealed class BoqDescriptionParser : IBoqDescriptionParser
{
    private static readonly (string Prefix, string DivisionName)[] CsiDivisions =
    [
        ("01", "General Requirements"), ("02", "Site Construction"), ("03", "Concrete"),
        ("04", "Masonry"), ("05", "Metals"), ("06", "Wood, Plastics & Composites"),
        ("07", "Thermal & Moisture Protection"), ("08", "Openings"), ("09", "Finishes"),
        ("10", "Specialties"), ("11", "Equipment"), ("12", "Furnishings"),
        ("13", "Special Construction"), ("14", "Conveying Systems"),
        ("21", "Fire Suppression"), ("22", "Plumbing"), ("23", "HVAC"),
        ("25", "Integrated Automation"), ("26", "Electrical"), ("27", "Communications"),
        ("28", "Electronic Safety & Security"), ("31", "Earthwork"),
        ("32", "Exterior Improvements"), ("33", "Utilities"), ("34", "Transportation"),
        ("35", "Waterway & Marine Construction"), ("40", "Process Integration"),
        ("41", "Material Processing & Handling"), ("42", "Process Heating & Cooling"),
        ("43", "Process Gas & Liquid Handling"), ("44", "Pollution & Waste Control"),
        ("45", "Industry-Specific Manufacturing"), ("46", "Water & Wastewater"),
        ("47", "Power Generation"), ("48", "Electrical Power Generation")
    ];

    private static readonly (string[] Patterns, string TradeName)[] TradePatterns =
    [
        (["earthwork", "excavation", "fill", "grading", "backfill", "cut", "embankment", "subgrade", "trenching"], "Earthworks"),
        (["concrete", "reinforcement", "rebar", "formwork", "pour", "precast", "cast-in-place", "curing", "cement", "ريانات", "خرسانة"], "Concrete"),
        (["masonry", "brick", "block", "stone", "plaster", "render", "طوب", "بلاط"], "Masonry"),
        (["steel", "metal", "iron", "structural steel", "rebar", "فراغات", "حديد"], "Metals/Structural"),
        (["wood", "timber", "plywood", "carpentry", "خشب"], "Wood/Carpentry"),
        (["insulation", "waterproofing", "damp-proofing", "roofing", "membrane", "عزل", "سقف"], "Thermal/Moisture"),
        (["door", "window", "glazing", "curtain wall", "باب", "نافذة", "زجاج"], "Openings/Doors/Windows"),
        (["paint", "coating", "finish", "plaster", "tile", "flooring", "ceiling", "wall finish", "دهان", "طلاء", "تشطيب", "بلاط"], "Finishes"),
        (["electrical", "wiring", "cable", "lighting", "switch", "panel", "generator", "transformer", "كهرباء", "إضاءة"], "Electrical"),
        (["plumbing", "pipe", "valve", "fixture", "drainage", "water supply", "sanitary", "سباكة", "مواسير", "صرف"], "Plumbing"),
        (["hvac", "air conditioning", "duct", "ventilation", "heating", "cooling", "chiller", "تكييف", "تهوية"], "HVAC"),
        (["fire", "sprinkler", "fire alarm", "fire protection", "fire suppression", "حريق", "إطفاء"], "Fire Protection"),
        (["road", "pavement", "asphalt", "subbase", "base course", "surface course", "kerb", "curb", "طرق", "رصف", "أسفلت"], "Roads/Paving"),
        (["landscape", "planting", "irrigation", "grass", "tree", "garden", "تنسيق", "زراعة"], "Landscaping"),
        (["piling", "foundation", "bored pile", "driven pile", "sheet pile", "pile cap", "أساسات", "خوازيق"], "Piling/Foundation"),
        (["survey", "setting out", "measurement", "leveling", "قياس", "مساحة"], "Surveying")
    ];

    private static readonly string[] DivisionPrefixPatterns =
        ["Division ", "Div ", "DIV ", "Sec ", "Section ", "Part ", "Ch "];

    private static readonly string[] ClassificationPrefixPatterns =
        ["Class ", "Classification ", "Cat ", "Category ", "Group ", "Trade ", "Type "];

    public ParsedClassification ParseFromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return new ParsedClassification(null, null, null, null, null);

        var trimmed = code.Trim();

        var csiMatch = CsiDivisions.FirstOrDefault(d => trimmed.StartsWith(d.Prefix, StringComparison.OrdinalIgnoreCase));
        if (csiMatch.Prefix is not null)
        {
            var divName = csiMatch.DivisionName;
            var remaining = trimmed.Length > 2 ? trimmed : null;
            return new ParsedClassification(csiMatch.Prefix, divName, remaining, null, null);
        }

        if (trimmed.Contains('.') || trimmed.Contains('-') || trimmed.Contains('_'))
        {
            var segments = trimmed.Split(['.', '-', '_'], 2, StringSplitOptions.RemoveEmptyEntries);
            var divCode = segments[0].Trim();
            var csiDiv = CsiDivisions.FirstOrDefault(d => d.Prefix == divCode);
            return new ParsedClassification(
                divCode,
                csiDiv.DivisionName ?? $"Division {divCode}",
                segments.Length > 1 ? segments[1].Trim() : null,
                null,
                null);
        }

        return new ParsedClassification(null, null, trimmed, null, null);
    }

    public ParsedClassification ParseFromDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return new ParsedClassification(null, null, null, null, null);

        var trimmed = description.Trim();
        string? extractedDivCode = null;
        string? extractedDivName = null;
        string? extractedClassCode = null;
        string? extractedClassName = null;
        var cleaned = trimmed;

        var divPrefix = DivisionPrefixPatterns.FirstOrDefault(p => trimmed.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        if (divPrefix is not null)
        {
            var idx = trimmed.IndexOf(divPrefix, StringComparison.OrdinalIgnoreCase);
            var afterPrefix = trimmed.Substring(idx + divPrefix.Length).Trim();
            var spaceIdx = afterPrefix.IndexOf(' ');
            if (spaceIdx > 0)
            {
                extractedDivCode = afterPrefix.Substring(0, spaceIdx).Trim();
                var rest = afterPrefix.Substring(spaceIdx + 1).Trim();
                var dashIdx = rest.IndexOf('-');
                if (dashIdx > 0)
                {
                    extractedDivName = rest.Substring(0, dashIdx).Trim();
                    cleaned = rest.Substring(dashIdx + 1).Trim();
                }
                else
                {
                    extractedDivName = rest;
                    cleaned = string.Empty;
                }
            }
            else
            {
                extractedDivCode = afterPrefix;
                cleaned = string.Empty;
            }
        }

        var classPrefix = ClassificationPrefixPatterns.FirstOrDefault(p => trimmed.Contains(p, StringComparison.OrdinalIgnoreCase));
        if (classPrefix is not null && extractedDivCode is null)
        {
            var idx = trimmed.IndexOf(classPrefix, StringComparison.OrdinalIgnoreCase);
            var afterPrefix = trimmed.Substring(idx + classPrefix.Length).Trim();
            var spaceIdx = afterPrefix.IndexOf(' ');
            if (spaceIdx > 0)
            {
                extractedClassCode = afterPrefix.Substring(0, spaceIdx).Trim();
                extractedClassName = afterPrefix.Substring(spaceIdx + 1).Trim();
                cleaned = trimmed.Substring(0, idx).Trim();
            }
        }

        if (extractedDivCode is null && extractedClassName is null)
        {
            foreach (var (patterns, tradeName) in TradePatterns)
            {
                if (patterns.Any(p => trimmed.Contains(p, StringComparison.OrdinalIgnoreCase)))
                {
                    extractedClassName = tradeName;
                    break;
                }
            }
        }

        if (extractedDivCode is null)
        {
            var codeStart = trimmed.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
            if (codeStart == 0 || (codeStart > 0 && trimmed[codeStart - 1] == ' '))
            {
                var numPart = trimmed.Substring(codeStart);
                var spaceAfterNum = numPart.IndexOf(' ');
                if (spaceAfterNum > 0 && spaceAfterNum <= 4)
                {
                    var potentialDiv = trimmed.Substring(codeStart, spaceAfterNum);
                    var csiDiv = CsiDivisions.FirstOrDefault(d => d.Prefix == potentialDiv);
                    if (csiDiv.Prefix is not null)
                    {
                        extractedDivCode = csiDiv.Prefix;
                        extractedDivName = csiDiv.DivisionName;
                    }
                }
            }
        }

        return new ParsedClassification(extractedDivCode, extractedDivName, extractedClassCode, extractedClassName, cleaned);
    }

    public ImportRow EnrichImportRow(ImportRow row, string? classificationColumn, string? divisionColumn, IReadOnlyDictionary<string, object> rawValues)
    {
        string? classification = null;
        string? division = null;
        var cleanedDescription = row.Description;

        if (!string.IsNullOrEmpty(classificationColumn) && rawValues.TryGetValue(classificationColumn, out var classVal))
            classification = classVal?.ToString()?.Trim();

        if (!string.IsNullOrEmpty(divisionColumn) && rawValues.TryGetValue(divisionColumn, out var divVal))
            division = divVal?.ToString()?.Trim();

        if (string.IsNullOrEmpty(classification) && string.IsNullOrEmpty(division))
        {
            var fromCode = ParseFromCode(row.Code);
            var fromDesc = ParseFromDescription(row.Description);

            if (string.IsNullOrEmpty(division) && fromCode.DivisionCode is not null)
            {
                division = $"{fromCode.DivisionCode} - {fromCode.DivisionName}";
                if (fromCode.ClassificationCode is not null && string.IsNullOrEmpty(classification))
                    classification = fromCode.ClassificationCode;
            }

            if (string.IsNullOrEmpty(division) && fromDesc.DivisionCode is not null)
            {
                division = $"{fromDesc.DivisionCode} - {fromDesc.DivisionName}";
                if (fromDesc.CleanedDescription is not null && fromDesc.CleanedDescription.Length > 0)
                    cleanedDescription = fromDesc.CleanedDescription;
            }

            if (string.IsNullOrEmpty(classification) && fromDesc.ClassificationName is not null)
                classification = fromDesc.ClassificationName;
        }

        return row with { Classification = classification, Division = division, Description = cleanedDescription };
    }
}
