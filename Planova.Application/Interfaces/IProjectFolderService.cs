namespace Planova.Application.Interfaces;

public interface IProjectFolderService
{
    Task<IReadOnlyList<string>> CreateFolderStructureAsync(string rootPath, CancellationToken ct);
    IReadOnlyList<string> GetStandardFolderNames();
}
