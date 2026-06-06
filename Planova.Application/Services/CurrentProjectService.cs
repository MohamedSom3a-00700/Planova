using Planova.Shared.Abstractions;

namespace Planova.Application.Services;

public class CurrentProjectService : ICurrentProjectService
{
    private ProjectContext? _currentProject;

    public ProjectContext? CurrentProject => _currentProject;

    public event EventHandler<ProjectContext?>? CurrentProjectChanged;

    public void SetProject(ProjectContext? project)
    {
        if (_currentProject == project)
            return;

        _currentProject = project;
        CurrentProjectChanged?.Invoke(this, project);
    }
}