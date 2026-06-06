using Planova.Application.Dto;
using Planova.Application.Exceptions;
using Planova.Application.Mappings;
using Planova.Application.Repositories;

namespace Planova.Application.Services;

public class SubcontractorService : ISubcontractorService
{
    private readonly ISubcontractorRepository _subcontractorRepository;

    public SubcontractorService(ISubcontractorRepository subcontractorRepository)
    {
        _subcontractorRepository = subcontractorRepository;
    }

    public async Task<IEnumerable<SubcontractorSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var subcontractors = await _subcontractorRepository.GetAllAsync(ct);
        return subcontractors.Select(s => s.ToSummaryDto());
    }

    public async Task<SubcontractorDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var subcontractor = await _subcontractorRepository.GetByIdAsync(id, ct);
        return subcontractor?.ToDetailDto();
    }

    public async Task<SubcontractorDetailDto> CreateAsync(CreateSubcontractorDto dto, CancellationToken ct = default)
    {
        if (await _subcontractorRepository.CodeExistsAsync(dto.Code, null, ct))
            throw new DuplicateEntityException("Subcontractor", "code", dto.Code);

        if (await _subcontractorRepository.NameExistsAsync(dto.Name, null, ct))
            throw new DuplicateEntityException("Subcontractor", "name", dto.Name);

        var subcontractor = new Domain.Entities.Subcontractor
        {
            Code = dto.Code,
            Name = dto.Name,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            OrganizationDetails = dto.OrganizationDetails,
            Trade = dto.Trade,
            LicenseNumber = dto.LicenseNumber,
            Logo = dto.Logo,
            Notes = dto.Notes,
        };

        var created = await _subcontractorRepository.AddAsync(subcontractor, ct);
        return (await _subcontractorRepository.GetByIdAsync(created.Id, ct))!.ToDetailDto();
    }

    public async Task<SubcontractorDetailDto> UpdateAsync(int id, UpdateSubcontractorDto dto, CancellationToken ct = default)
    {
        var subcontractor = await _subcontractorRepository.GetByIdAsync(id, ct);
        if (subcontractor == null)
            throw new EntityNotFoundException("Subcontractor", id);

        if (await _subcontractorRepository.CodeExistsAsync(dto.Code, id, ct))
            throw new DuplicateEntityException("Subcontractor", "code", dto.Code);

        if (await _subcontractorRepository.NameExistsAsync(dto.Name, id, ct))
            throw new DuplicateEntityException("Subcontractor", "name", dto.Name);

        subcontractor.Code = dto.Code;
        subcontractor.Name = dto.Name;
        subcontractor.ContactEmail = dto.ContactEmail;
        subcontractor.ContactPhone = dto.ContactPhone;
        subcontractor.OrganizationDetails = dto.OrganizationDetails;
        subcontractor.Trade = dto.Trade;
        subcontractor.LicenseNumber = dto.LicenseNumber;
        subcontractor.Logo = dto.Logo;
        subcontractor.Notes = dto.Notes;
        subcontractor.UpdatedAt = DateTime.UtcNow;

        await _subcontractorRepository.UpdateAsync(subcontractor, ct);
        return (await _subcontractorRepository.GetByIdAsync(id, ct))!.ToDetailDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var subcontractor = await _subcontractorRepository.GetByIdAsync(id, ct);
        if (subcontractor == null)
            throw new EntityNotFoundException("Subcontractor", id);

        if (await _subcontractorRepository.HasLinkedProjectsAsync(id, ct))
            throw new EntityInUseException("Subcontractor", "project");

        await _subcontractorRepository.DeleteAsync(subcontractor, ct);
    }

    public async Task<IEnumerable<SubcontractorSummaryDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        var subcontractors = await _subcontractorRepository.SearchAsync(query, ct);
        return subcontractors.Select(s => s.ToSummaryDto());
    }
}