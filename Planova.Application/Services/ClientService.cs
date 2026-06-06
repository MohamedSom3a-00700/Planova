using Planova.Application.Dto;
using Planova.Application.Exceptions;
using Planova.Application.Mappings;
using Planova.Application.Repositories;

namespace Planova.Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<IEnumerable<ClientSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var clients = await _clientRepository.GetAllAsync(ct);
        return clients.Select(c => c.ToSummaryDto());
    }

    public async Task<ClientDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct);
        return client?.ToDetailDto();
    }

    public async Task<ClientDetailDto> CreateAsync(CreateClientDto dto, CancellationToken ct = default)
    {
        if (await _clientRepository.CodeExistsAsync(dto.Code, null, ct))
            throw new DuplicateEntityException("Client", "code", dto.Code);

        if (await _clientRepository.NameExistsAsync(dto.Name, null, ct))
            throw new DuplicateEntityException("Client", "name", dto.Name);

        var client = new Domain.Entities.Client
        {
            Code = dto.Code,
            Name = dto.Name,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            OrganizationDetails = dto.OrganizationDetails,
            Logo = dto.Logo,
            Notes = dto.Notes,
        };

        var created = await _clientRepository.AddAsync(client, ct);
        return (await _clientRepository.GetByIdAsync(created.Id, ct))!.ToDetailDto();
    }

    public async Task<ClientDetailDto> UpdateAsync(int id, UpdateClientDto dto, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct);
        if (client == null)
            throw new EntityNotFoundException("Client", id);

        if (await _clientRepository.CodeExistsAsync(dto.Code, id, ct))
            throw new DuplicateEntityException("Client", "code", dto.Code);

        if (await _clientRepository.NameExistsAsync(dto.Name, id, ct))
            throw new DuplicateEntityException("Client", "name", dto.Name);

        client.Code = dto.Code;
        client.Name = dto.Name;
        client.ContactEmail = dto.ContactEmail;
        client.ContactPhone = dto.ContactPhone;
        client.OrganizationDetails = dto.OrganizationDetails;
        client.Logo = dto.Logo;
        client.Notes = dto.Notes;
        client.UpdatedAt = DateTime.UtcNow;

        await _clientRepository.UpdateAsync(client, ct);
        return (await _clientRepository.GetByIdAsync(id, ct))!.ToDetailDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct);
        if (client == null)
            throw new EntityNotFoundException("Client", id);

        if (await _clientRepository.HasLinkedProjectsAsync(id, ct))
            throw new EntityInUseException("Client", "project");

        await _clientRepository.DeleteAsync(client, ct);
    }

    public async Task<IEnumerable<ClientSummaryDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        var clients = await _clientRepository.SearchAsync(query, ct);
        return clients.Select(c => c.ToSummaryDto());
    }
}
