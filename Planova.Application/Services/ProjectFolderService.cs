using Planova.Application.Interfaces;

namespace Planova.Application.Services;

public sealed class ProjectFolderService : IProjectFolderService
{
    private static readonly string[] StandardFolders =
    [
        "Contract",
        "BOQ",
        "Drawing",
        "Specification",
        "Schedule",
        "Claim",
        "Reports",
        "Correspondence",
        "Photo",
        "Meeting & Presentations",
        "Archive",
        "Lookahead",
        "Logos"
    ];

    public Task<IReadOnlyList<string>> CreateFolderStructureAsync(string rootPath, CancellationToken ct)
    {
        var created = new List<string>();

        try
        {
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
                created.Add(rootPath);
            }

            foreach (var folder in StandardFolders)
            {
                ct.ThrowIfCancellationRequested();

                var fullPath = Path.Combine(rootPath, folder);
                try
                {
                    Directory.CreateDirectory(fullPath);
                    created.Add(fullPath);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new InvalidOperationException(
                        $"Permission denied: Cannot create folder '{folder}'. Please check folder permissions.");
                }
                catch (IOException ex) when (
                    ex.Message.Contains("disk full", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("insufficient space", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Disk full: Cannot create folder '{folder}'. Please free up disk space.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to create project folder structure: {ex.Message}", ex);
        }

        return Task.FromResult<IReadOnlyList<string>>(created.AsReadOnly());
    }

    public IReadOnlyList<string> GetStandardFolderNames()
    {
        return StandardFolders.AsReadOnly();
    }
}
