using System.Collections.Generic;

namespace Planova.Application.Services;

public static class DocumentTypeRegistry
{
    private static readonly HashSet<string> StudioRequiredTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Boq",
        "Spec",
        "Contract"
    };

    public static bool IsLockedType(string documentType)
    {
        return StudioRequiredTypes.Contains(documentType);
    }

    public static IReadOnlySet<string> LockedTypes => StudioRequiredTypes;
}