namespace Planova.Boq.Application.Dto;

public record CsvImportOptions(
    bool HasHeaders,
    string CodeColumn,
    string DescriptionColumn,
    string UnitColumn,
    string QuantityColumn,
    string RateColumn,
    string? LevelColumn,
    string? ParentIdColumn,
    string? ClassificationColumn,
    string? DivisionColumn,
    string Delimiter = ","
);
