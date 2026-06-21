namespace Planova.Boq.Application.Models;

public class BoqPreviewRow
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public string SourceSheet { get; set; } = string.Empty;
    public int RowIndex { get; set; }
}
