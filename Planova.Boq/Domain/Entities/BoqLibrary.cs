using Planova.Boq.Domain.Enums;

namespace Planova.Boq.Domain.Entities;

public class BoqLibrary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LibraryType LibraryType { get; set; } = LibraryType.UserDefined;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public ICollection<BoqLibraryItem> Items { get; set; } = new List<BoqLibraryItem>();
}
