using Planova.Application.Dto;
using Planova.Application.Exceptions;
using Planova.Application.Mappings;
using Planova.Application.Repositories;

namespace Planova.Application.Services;

public class ConsultantService : IConsultantService
{
    private readonly IConsultantRepository _consultantRepository;

    public ConsultantService(IConsultantRepository consultantRepository)
    {
        _consultantRepository = consultantRepository;
    }

    public async Task<IEnumerable<ConsultantSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var consultants = await _consultantRepository.GetAllAsync(ct);
        return consultants.Select(c => c.ToSummaryDto());
    }

    public async Task<ConsultantDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var consultant = await _consultantRepository.GetByIdAsync(id, ct);
        return consultant?.ToDetailDto();
    }

    public async Task<ConsultantDetailDto> CreateAsync(CreateConsultantDto dto, CancellationToken ct = default)
    {
        if (await _consultantRepository.CodeExistsAsync(dto.Code, null, ct))
            throw new DuplicateEntityException("Consultant", "code", dto.Code);

        if (await _consultantRepository.NameExistsAsync(dto.Name, null, ct))
            throw new DuplicateEntityException("Consultant", "name", dto.Name);

        var consultant = new Domain.Entities.Consultant
        {
            Code = dto.Code,
            Name = dto.Name,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            OrganizationDetails = dto.OrganizationDetails,
            Logo = dto.Logo,
            Notes = dto.Notes,
        };

        var created = await _consultantRepository.AddAsync(consultant, ct);
        return (await _consultantRepository.GetByIdAsync(created.Id, ct))!.ToDetailDto();
    }

    public async Task<ConsultantDetailDto> UpdateAsync(int id, UpdateConsultantDto dto, CancellationToken ct = default)
    {
        var consultant = await _consultantRepository.GetByIdAsync(id, ct);
        if (consultant == null)
            throw new EntityNotFoundException("Consultant", id);

        if (await _consultantRepository.CodeExistsAsync(dto.Code, id, ct))
            throw new DuplicateEntityException("Consultant", "code", dto.Code);

        if (await _consultantRepository.NameExistsAsync(dto.Name, id, ct))
            throw new DuplicateEntityException("Consultant", "name", dto.Name);

        consultant.Code = dto.Code;
        consultant.Name = dto.Name;
        consultant.ContactEmail = dto.ContactEmail;
        consultant.ContactPhone = dto.ContactPhone;
        consultant.OrganizationDetails = dto.OrganizationDetails;
        consultant.Logo = dto.Logo;
        consultant.Notes = dto.Notes;
        consultant.UpdatedAt = DateTime.UtcNow;

        await _consultantRepository.UpdateAsync(consultant, ct);
        return (await _consultantRepository.GetByIdAsync(id, ct))!.ToDetailDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var consultant = await _consultantRepository.GetByIdAsync(id, ct);
        if (consultant == null)
            throw new EntityNotFoundException("Consultant", id);

        if (await _consultantRepository.HasLinkedProjectsAsync(id, ct))
            throw new EntityInUseException("Consultant", "project");

        await _consultantRepository.DeleteAsync(consultant, ct);
    }

    public async Task<IEnumerable<ConsultantSummaryDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        var consultants = await _consultantRepository.SearchAsync(query, ct);
        return consultants.Select(c => c.ToSummaryDto());
    }
}
