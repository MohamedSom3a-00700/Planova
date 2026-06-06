using FluentAssertions;
using Moq;
using Planova.Application.Dto;
using Planova.Application.Exceptions;
using Planova.Application.Repositories;
using Planova.Application.Services;
using Planova.Domain.Entities;

namespace Application.Tests;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepo;
    private readonly Mock<IClientRepository> _clientRepo;
    private readonly Mock<IContractRepository> _contractRepo;
    private readonly Mock<IContractorRepository> _contractorRepo;
    private readonly Mock<ISubcontractorRepository> _subcontractorRepo;
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _projectRepo = new Mock<IProjectRepository>();
        _clientRepo = new Mock<IClientRepository>();
        _contractRepo = new Mock<IContractRepository>();
        _contractorRepo = new Mock<IContractorRepository>();
        _subcontractorRepo = new Mock<ISubcontractorRepository>();
        _service = new ProjectService(_projectRepo.Object, _clientRepo.Object, _contractRepo.Object, _contractorRepo.Object, _subcontractorRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProjects()
    {
        _projectRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Project> { new() { Id = 1, Code = "P1", Name = "Test", Status = "Draft" } });

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_Throws()
    {
        _projectRepo.Setup(r => r.CodeExistsAsync("P1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new CreateProjectDto("P1", "Test", null, null, null, null, null, null, null, null, null);

        Func<Task> act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_Throws()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        Func<Task> act = () => _service.DeleteAsync(999);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ProjectWithContracts_Throws()
    {
        var project = new Project { Id = 1, Code = "P1", Name = "Test", Contracts = new List<Contract> { new() } };
        _projectRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        Func<Task> act = () => _service.DeleteAsync(1);
        await act.Should().ThrowAsync<EntityInUseException>();
    }

    [Fact]
    public async Task ChangeStatusAsync_InvalidTransition_Throws()
    {
        var project = new Project { Id = 1, Code = "P1", Name = "Test", Status = "Draft" };
        _projectRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        Func<Task> act = () => _service.ChangeStatusAsync(1, "Completed");
        await act.Should().ThrowAsync<InvalidTransitionException>();
    }
}
