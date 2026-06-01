using Planova.Application.Dto;
using Planova.Application.Exceptions;
using Planova.Application.Mappings;
using Planova.Application.Repositories;
using Planova.Domain.ValueObjects;

namespace Planova.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IContractRepository _contractRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IClientRepository clientRepository,
        IContractRepository contractRepository)
    {
        _projectRepository = projectRepository;
        _clientRepository = clientRepository;
        _contractRepository = contractRepository;
    }

    public async Task<IEnumerable<ProjectSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var projects = await _projectRepository.GetAllAsync(ct);
        return projects.Select(p => p.ToSummaryDto());
    }

    public async Task<ProjectDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct);
        return project?.ToDetailDto();
    }

    public async Task<ProjectDetailDto> CreateAsync(CreateProjectDto dto, CancellationToken ct = default)
    {
        if (await _projectRepository.CodeExistsAsync(dto.Code, null, ct))
            throw new DuplicateEntityException("Project", "code", dto.Code);

        var project = new Domain.Entities.Project
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            Status = ProjectStatus.Draft.Value,
            StartDate = ParseDate(dto.StartDate),
            FinishDate = ParseDate(dto.FinishDate),
            Currency = dto.Currency,
            Location = dto.Location,
            ClientId = dto.ClientId,
            Notes = dto.Notes,
        };

        ValidateProjectDates(project);
        await ValidateClientExists(dto.ClientId, ct);

        var created = await _projectRepository.AddAsync(project, ct);
        return (await _projectRepository.GetByIdAsync(created.Id, ct))!.ToDetailDto();
    }

    public async Task<ProjectDetailDto> UpdateAsync(int id, UpdateProjectDto dto, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct);
        if (project == null)
            throw new EntityNotFoundException("Project", id);

        if (await _projectRepository.CodeExistsAsync(dto.Code, id, ct))
            throw new DuplicateEntityException("Project", "code", dto.Code);

        project.Code = dto.Code;
        project.Name = dto.Name;
        project.Description = dto.Description;
        project.StartDate = ParseDate(dto.StartDate);
        project.FinishDate = ParseDate(dto.FinishDate);
        project.Currency = dto.Currency;
        project.Location = dto.Location;
        project.ClientId = dto.ClientId;
        project.Notes = dto.Notes;
        project.UpdatedAt = DateTime.UtcNow;

        ValidateProjectDates(project);
        await ValidateClientExists(dto.ClientId, ct);

        await _projectRepository.UpdateAsync(project, ct);
        return (await _projectRepository.GetByIdAsync(id, ct))!.ToDetailDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct);
        if (project == null)
            throw new EntityNotFoundException("Project", id);

        if (project.Contracts is { Count: > 0 })
            throw new EntityInUseException("Project", "contract");

        await _projectRepository.DeleteAsync(project, ct);
    }

    public async Task<ProjectDetailDto> ChangeStatusAsync(int id, string newStatus, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct);
        if (project == null)
            throw new EntityNotFoundException("Project", id);

        var current = ProjectStatus.FromString(project.Status);
        var target = ProjectStatus.FromString(newStatus);

        if (!current.CanTransitionTo(target))
            throw new InvalidTransitionException(project.Status, newStatus);

        project.Status = target.Value;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project, ct);
        return (await _projectRepository.GetByIdAsync(id, ct))!.ToDetailDto();
    }

    public async Task<IEnumerable<ProjectSummaryDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        var projects = await _projectRepository.SearchAsync(query, ct);
        return projects.Select(p => p.ToSummaryDto());
    }

    public async Task<IEnumerable<ProjectSummaryDto>> GetByStatusAsync(string status, CancellationToken ct = default)
    {
        var projects = await _projectRepository.GetByStatusAsync(status, ct);
        return projects.Select(p => p.ToSummaryDto());
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default)
    {
        var projects = await _projectRepository.GetAllAsync(ct);
        var totalClients = await _clientRepository.GetCountAsync(ct);
        var totalContracts = await _contractRepository.GetCountAsync(ct);

        var byStatus = projects
            .GroupBy(p => p.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var recent = projects
            .OrderByDescending(p => p.UpdatedAt)
            .Take(5)
            .Select(p => new RecentActivityItem("Project", p.Name, "Updated", p.UpdatedAt))
            .ToList();

        return new DashboardSummaryDto(
            projects.Count(),
            totalClients,
            totalContracts,
            byStatus,
            recent
        );
    }

    private static DateTime? ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;
        return DateTime.TryParse(dateStr, out var dt) ? dt : null;
    }

    private static void ValidateProjectDates(Domain.Entities.Project project)
    {
        if (project.StartDate.HasValue && project.FinishDate.HasValue &&
            project.FinishDate < project.StartDate)
        {
            throw new ValidationException("Finish date must be on or after start date.");
        }
    }

    private async Task ValidateClientExists(int? clientId, CancellationToken ct)
    {
        if (clientId.HasValue)
        {
            var client = await _clientRepository.GetByIdAsync(clientId.Value, ct);
            if (client == null)
                throw new EntityNotFoundException("Client", clientId.Value);
        }
    }
}
