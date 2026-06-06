namespace Planova.Shared.Abstractions;

public sealed record ProjectContext(
    int Id,
    string Code,
    string Name
);

public interface ICurrentProjectService
{
    ProjectContext? CurrentProject { get; }
    event EventHandler<ProjectContext?>? CurrentProjectChanged;
    void SetProject(ProjectContext? project);
}