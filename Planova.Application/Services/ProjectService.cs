using System.IO;
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
    private readonly IContractorRepository _contractorRepository;
    private readonly ISubcontractorRepository _subcontractorRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IClientRepository clientRepository,
        IContractRepository contractRepository,
        IContractorRepository contractorRepository,
        ISubcontractorRepository subcontractorRepository)
    {
        _projectRepository = projectRepository;
        _clientRepository = clientRepository;
        _contractRepository = contractRepository;
        _contractorRepository = contractorRepository;
        _subcontractorRepository = subcontractorRepository;
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
            StartDate = dto.StartDate,
            FinishDate = dto.FinishDate,
            Currency = dto.Currency,
            Location = dto.Location,
            ClientId = dto.ClientId,
            ContractorId = dto.ContractorId,
            SubcontractorId = dto.SubcontractorId,
            Notes = dto.Notes,
            DocumentsFolder = dto.DocumentsFolder,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
        };

        ValidateCoordinates(dto.Latitude, dto.Longitude);

        if (!string.IsNullOrWhiteSpace(dto.LogoSourcePath))
        {
            if (!File.Exists(dto.LogoSourcePath))
                throw new ValidationException($"Logo file not found: {dto.LogoSourcePath}");

            var logoFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Planova", "Projects");
            Directory.CreateDirectory(logoFolder);
            var ext = Path.GetExtension(dto.LogoSourcePath);
            var logoDest = Path.Combine(logoFolder, $"logo_{Guid.NewGuid()}{ext}");
            File.Copy(dto.LogoSourcePath, logoDest, overwrite: false);
            project.LogoPath = logoDest;
        }

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
        project.StartDate = dto.StartDate;
        project.FinishDate = dto.FinishDate;
        project.Currency = dto.Currency;
        project.Location = dto.Location;
        project.ClientId = dto.ClientId;
        project.ContractorId = dto.ContractorId;
        project.SubcontractorId = dto.SubcontractorId;
        project.Notes = dto.Notes;
        project.DocumentsFolder = dto.DocumentsFolder;
        project.Latitude = dto.Latitude;
        project.Longitude = dto.Longitude;
        project.QrCodePath = dto.QrCodePath ?? project.QrCodePath;
        project.UpdatedAt = DateTime.UtcNow;

        ValidateCoordinates(dto.Latitude, dto.Longitude);

        if (!string.IsNullOrWhiteSpace(dto.LogoSourcePath))
        {
            if (!File.Exists(dto.LogoSourcePath))
                throw new ValidationException($"Logo file not found: {dto.LogoSourcePath}");

            var logoFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Planova", "Projects");
            Directory.CreateDirectory(logoFolder);
            var ext = Path.GetExtension(dto.LogoSourcePath);
            var logoDest = Path.Combine(logoFolder, $"logo_{Guid.NewGuid()}{ext}");
            File.Copy(dto.LogoSourcePath, logoDest, overwrite: false);
            project.LogoPath = logoDest;
        }

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
            0,
            0,
            0,
            0,
            byStatus,
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            recent
        );
    }

    private static void ValidateCoordinates(double? latitude, double? longitude)
    {
        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            throw new ValidationException("Latitude must be between -90 and 90.");

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            throw new ValidationException("Longitude must be between -180 and 180.");
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
