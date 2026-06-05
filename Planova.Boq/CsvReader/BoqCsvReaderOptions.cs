namespace Planova.Boq.CsvReader;

public class BoqCsvReaderOptions
{
    public string Delimiter { get; set; } = ",";
    public bool HasHeaders { get; set; } = true;
    public string CodeColumn { get; set; } = "Code";
    public string DescriptionColumn { get; set; } = "Description";
    public string UnitColumn { get; set; } = "Unit";
    public string QuantityColumn { get; set; } = "Quantity";
    public string RateColumn { get; set; } = "Rate";
    public string? LevelColumn { get; set; }
    public string? ParentIdColumn { get; set; }
}
