using Planova.Application.Dto;
using Planova.Application.Exceptions;
using Planova.Application.Mappings;
using Planova.Application.Repositories;

namespace Planova.Application.Services;

public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IClientRepository _clientRepository;

    public ContractService(
        IContractRepository contractRepository,
        IProjectRepository projectRepository,
        IClientRepository clientRepository)
    {
        _contractRepository = contractRepository;
        _projectRepository = projectRepository;
        _clientRepository = clientRepository;
    }

    public async Task<IEnumerable<ContractSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var contracts = await _contractRepository.GetAllAsync(ct);
        return contracts.Select(c => c.ToSummaryDto());
    }

    public async Task<ContractDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var contract = await _contractRepository.GetByIdAsync(id, ct);
        return contract?.ToDetailDto();
    }

    public async Task<ContractDetailDto> CreateAsync(CreateContractDto dto, CancellationToken ct = default)
    {
        if (await _contractRepository.NumberExistsAsync(dto.Number, null, ct))
            throw new DuplicateEntityException("Contract", "number", dto.Number);

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId, ct);
        if (project == null)
            throw new EntityNotFoundException("Project", dto.ProjectId);

        var client = await _clientRepository.GetByIdAsync(dto.ClientId, ct);
        if (client == null)
            throw new EntityNotFoundException("Client", dto.ClientId);

        ValidateContractDates(dto.CommencementDate, dto.CompletionDate);

        var contract = new Domain.Entities.Contract
        {
            Number = dto.Number,
            Title = dto.Title,
            Value = dto.Value,
            Currency = dto.Currency,
            AwardDate = dto.AwardDate,
            CommencementDate = dto.CommencementDate,
            CompletionDate = dto.CompletionDate,
            Status = dto.Status,
            Notes = dto.Notes,
            ProjectId = dto.ProjectId,
            ClientId = dto.ClientId,
        };

        var created = await _contractRepository.AddAsync(contract, ct);
        return (await _contractRepository.GetByIdAsync(created.Id, ct))!.ToDetailDto();
    }

    public async Task<ContractDetailDto> UpdateAsync(int id, UpdateContractDto dto, CancellationToken ct = default)
    {
        var contract = await _contractRepository.GetByIdAsync(id, ct);
        if (contract == null)
            throw new EntityNotFoundException("Contract", id);

        if (await _contractRepository.NumberExistsAsync(dto.Number, id, ct))
            throw new DuplicateEntityException("Contract", "number", dto.Number);

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId, ct);
        if (project == null)
            throw new EntityNotFoundException("Project", dto.ProjectId);

        var client = await _clientRepository.GetByIdAsync(dto.ClientId, ct);
        if (client == null)
            throw new EntityNotFoundException("Client", dto.ClientId);

        ValidateContractDates(dto.CommencementDate, dto.CompletionDate);

        contract.Number = dto.Number;
        contract.Title = dto.Title;
        contract.Value = dto.Value;
        contract.Currency = dto.Currency;
        contract.AwardDate = dto.AwardDate;
        contract.CommencementDate = dto.CommencementDate;
        contract.CompletionDate = dto.CompletionDate;
        contract.Status = dto.Status;
        contract.Notes = dto.Notes;
        contract.ProjectId = dto.ProjectId;
        contract.ClientId = dto.ClientId;
        contract.UpdatedAt = DateTime.UtcNow;

        await _contractRepository.UpdateAsync(contract, ct);
        return (await _contractRepository.GetByIdAsync(id, ct))!.ToDetailDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var contract = await _contractRepository.GetByIdAsync(id, ct);
        if (contract == null)
            throw new EntityNotFoundException("Contract", id);

        await _contractRepository.DeleteAsync(contract, ct);
    }

    public async Task<IEnumerable<ContractSummaryDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        var contracts = await _contractRepository.SearchAsync(query, ct);
        return contracts.Select(c => c.ToSummaryDto());
    }

    public async Task<IEnumerable<ContractSummaryDto>> GetByProjectAsync(int projectId, CancellationToken ct = default)
    {
        var contracts = await _contractRepository.GetByProjectAsync(projectId, ct);
        return contracts.Select(c => c.ToSummaryDto());
    }

    public async Task<IEnumerable<ContractSummaryDto>> GetByClientAsync(int clientId, CancellationToken ct = default)
    {
        var contracts = await _contractRepository.GetByClientAsync(clientId, ct);
        return contracts.Select(c => c.ToSummaryDto());
    }

    private static void ValidateContractDates(DateTime? commencement, DateTime? completion)
    {
        if (commencement.HasValue && completion.HasValue && completion < commencement)
        {
            throw new ValidationException("Completion date must be on or after commencement date.");
        }
    }
}
