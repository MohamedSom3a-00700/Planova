using FluentAssertions;
using Moq;
using Planova.Application.Dto;
using Planova.Application.Exceptions;
using Planova.Application.Repositories;
using Planova.Application.Services;
using Planova.Domain.Entities;

namespace Application.Tests;

public class ContractServiceTests
{
    private readonly Mock<IContractRepository> _contractRepo;
    private readonly Mock<IProjectRepository> _projectRepo;
    private readonly Mock<IClientRepository> _clientRepo;
    private readonly ContractService _service;

    public ContractServiceTests()
    {
        _contractRepo = new Mock<IContractRepository>();
        _projectRepo = new Mock<IProjectRepository>();
        _clientRepo = new Mock<IClientRepository>();
        _service = new ContractService(_contractRepo.Object, _projectRepo.Object, _clientRepo.Object);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNumber_Throws()
    {
        _contractRepo.Setup(r => r.NumberExistsAsync("CTR001", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new CreateContractDto("CTR001", "Test", null, null, null, null, null, null, null, 1, 1);

        Func<Task> act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task CreateAsync_MissingProject_Throws()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var dto = new CreateContractDto("CTR001", "Test", null, null, null, null, null, null, null, 999, 1);

        Func<Task> act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_MissingClient_Throws()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Project { Id = 1, Code = "P1", Name = "Test" });
        _clientRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var dto = new CreateContractDto("CTR001", "Test", null, null, null, null, null, null, null, 1, 999);

        Func<Task> act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
