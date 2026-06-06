using Planova.Application.Dto;
using Planova.Application.Exceptions;
using Planova.Application.Mappings;
using Planova.Application.Repositories;

namespace Planova.Application.Services;

public class ContractorService : IContractorService
{
    private readonly IContractorRepository _contractorRepository;

    public ContractorService(IContractorRepository contractorRepository)
    {
        _contractorRepository = contractorRepository;
    }

    public async Task<IEnumerable<ContractorSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var contractors = await _contractorRepository.GetAllAsync(ct);
        return contractors.Select(c => c.ToSummaryDto());
    }

    public async Task<ContractorDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var contractor = await _contractorRepository.GetByIdAsync(id, ct);
        return contractor?.ToDetailDto();
    }

    public async Task<ContractorDetailDto> CreateAsync(CreateContractorDto dto, CancellationToken ct = default)
    {
        if (await _contractorRepository.CodeExistsAsync(dto.Code, null, ct))
            throw new DuplicateEntityException("Contractor", "code", dto.Code);

        if (await _contractorRepository.NameExistsAsync(dto.Name, null, ct))
            throw new DuplicateEntityException("Contractor", "name", dto.Name);

        var contractor = new Domain.Entities.Contractor
        {
            Code = dto.Code,
            Name = dto.Name,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            OrganizationDetails = dto.OrganizationDetails,
            Logo = dto.Logo,
            Notes = dto.Notes,
        };

        var created = await _contractorRepository.AddAsync(contractor, ct);
        return (await _contractorRepository.GetByIdAsync(created.Id, ct))!.ToDetailDto();
    }

    public async Task<ContractorDetailDto> UpdateAsync(int id, UpdateContractorDto dto, CancellationToken ct = default)
    {
        var contractor = await _contractorRepository.GetByIdAsync(id, ct);
        if (contractor == null)
            throw new EntityNotFoundException("Contractor", id);

        if (await _contractorRepository.CodeExistsAsync(dto.Code, id, ct))
            throw new DuplicateEntityException("Contractor", "code", dto.Code);

        if (await _contractorRepository.NameExistsAsync(dto.Name, id, ct))
            throw new DuplicateEntityException("Contractor", "name", dto.Name);

        contractor.Code = dto.Code;
        contractor.Name = dto.Name;
        contractor.ContactEmail = dto.ContactEmail;
        contractor.ContactPhone = dto.ContactPhone;
        contractor.OrganizationDetails = dto.OrganizationDetails;
        contractor.Logo = dto.Logo;
        contractor.Notes = dto.Notes;
        contractor.UpdatedAt = DateTime.UtcNow;

        await _contractorRepository.UpdateAsync(contractor, ct);
        return (await _contractorRepository.GetByIdAsync(id, ct))!.ToDetailDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var contractor = await _contractorRepository.GetByIdAsync(id, ct);
        if (contractor == null)
            throw new EntityNotFoundException("Contractor", id);

        if (await _contractorRepository.HasLinkedProjectsAsync(id, ct))
            throw new EntityInUseException("Contractor", "project");

        await _contractorRepository.DeleteAsync(contractor, ct);
    }

    public async Task<IEnumerable<ContractorSummaryDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        var contractors = await _contractorRepository.SearchAsync(query, ct);
        return contractors.Select(c => c.ToSummaryDto());
    }
}