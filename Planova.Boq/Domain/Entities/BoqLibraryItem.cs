namespace Planova.Boq.Domain.Entities;

public class BoqLibraryItem
{
    public Guid Id { get; set; }
    public Guid LibraryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal DefaultRate { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }

    public BoqLibrary Library { get; set; } = null!;
}
