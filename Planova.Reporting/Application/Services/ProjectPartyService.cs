using Planova.Reporting.Application.Dto;
using Planova.Reporting.Application.Mappings;
using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;

namespace Planova.Reporting.Application.Services;

public class ProjectPartyService : IProjectPartyService
{
    private readonly IProjectPartyRepository _repository;

    public ProjectPartyService(IProjectPartyRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProjectPartyDto>> GetPartiesAsync(int projectId, CancellationToken ct = default)
    {
        var parties = await _repository.GetByProjectAsync(projectId, ct);
        return parties.Select(p => p.ToDto()).ToList();
    }

    public async Task<List<ProjectPartyDto>> GetAllPartiesAsync(CancellationToken ct = default)
    {
        var parties = await _repository.GetAllAsync(ct);
        var projectNames = await _repository.GetProjectNamesAsync(ct);
        return parties
            .OrderBy(p => projectNames.GetValueOrDefault(p.ProjectId, string.Empty))
            .ThenBy(p => p.DisplayOrder)
            .Select(p => p.ToDto() with { ProjectName = projectNames.GetValueOrDefault(p.ProjectId, "Unknown") })
            .ToList();
    }

    public async Task<ProjectPartyDto> GetClientAsync(int projectId, CancellationToken ct = default)
    {
        var client = await _repository.GetClientAsync(projectId, ct);
        return client?.ToDto() ?? throw new InvalidOperationException("No client configured for this project.");
    }

    public async Task<ProjectPartyDto> GetMainContractorAsync(int projectId, CancellationToken ct = default)
    {
        var contractor = await _repository.GetMainContractorAsync(projectId, ct);
        return contractor?.ToDto() ?? throw new InvalidOperationException("No main contractor configured for this project.");
    }

    public async Task<List<ProjectPartyDto>> GetSubContractorsAsync(int projectId, CancellationToken ct = default)
    {
        var subs = await _repository.GetSubContractorsAsync(projectId, ct);
        return subs.Select(p => p.ToDto()).ToList();
    }

    public async Task<ProjectPartyDto> SavePartyAsync(int projectId, SavePartyRequest request, CancellationToken ct = default)
    {
        ProjectParty? party;

        var role = Enum.Parse<PartyRole>(request.Role);

        if (request.Id.HasValue)
        {
            party = await _repository.GetByIdAsync(request.Id.Value, ct);
            if (party is null)
                throw new InvalidOperationException($"Party {request.Id} not found.");

            if (role == PartyRole.Client || role == PartyRole.MainContractor)
            {
                var existing = await _repository.GetByProjectAsync(projectId, ct);
                var duplicate = existing.FirstOrDefault(p => p.Role == role && p.Id != request.Id.Value);
                if (duplicate is not null)
                    throw new InvalidOperationException($"This project already has a {request.Role}. Only one {request.Role} is allowed per project.");
            }

            party.Name = request.Name;
            party.Role = role;
            party.Address = request.Address;
            party.ContactPerson = request.ContactPerson;
            party.ContactEmail = request.ContactEmail;
            party.ContactPhone = request.ContactPhone;
            party.DisplayOrder = request.DisplayOrder;
            party.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(party, ct);
        }
        else
        {
            if (role == PartyRole.Client || role == PartyRole.MainContractor)
            {
                var existing = await _repository.GetByProjectAsync(projectId, ct);
                if (existing.Any(p => p.Role == role))
                    throw new InvalidOperationException($"This project already has a {request.Role}. Only one {request.Role} is allowed per project.");
            }

            party = new ProjectParty
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Role = role,
                Name = request.Name,
                Address = request.Address,
                ContactPerson = request.ContactPerson,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                DisplayOrder = request.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(party, ct);
        }

        return party.ToDto();
    }

    public async Task DeletePartyAsync(Guid partyId, CancellationToken ct = default)
    {
        var party = await _repository.GetByIdAsync(partyId, ct);
        if (party != null)
            await _repository.DeleteAsync(party, ct);
    }

    public async Task<string> UploadLogoAsync(Guid partyId, string fileName, Stream fileStream, CancellationToken ct = default)
    {
        var party = await _repository.GetByIdAsync(partyId, ct);
        if (party == null)
            throw new InvalidOperationException($"Party {partyId} not found.");

        var logoDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Planova", "Projects", party.ProjectId.ToString(), "Parties");

        Directory.CreateDirectory(logoDir);
        var ext = Path.GetExtension(fileName);
        var logoFileName = $"{partyId:N}{ext}";
        var logoPath = Path.Combine(logoDir, logoFileName);

        using (var fs = File.Create(logoPath))
        {
            await fileStream.CopyToAsync(fs, ct);
        }

        party.LogoPath = logoPath;
        party.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(party, ct);

        return logoPath;
    }
}
