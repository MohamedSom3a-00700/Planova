using FluentAssertions;
using Moq;
using Planova.Application.Dto;
using Planova.Application.Exceptions;
using Planova.Application.Repositories;
using Planova.Application.Services;
using Planova.Domain.Entities;

namespace Application.Tests;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _repo;
    private readonly ClientService _service;

    public ClientServiceTests()
    {
        _repo = new Mock<IClientRepository>();
        _service = new ClientService(_repo.Object);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_Throws()
    {
        _repo.Setup(r => r.CodeExistsAsync("C1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new CreateClientDto("C1", "Test", null, null, null, null, null);

        Func<Task> act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_Throws()
    {
        _repo.Setup(r => r.NameExistsAsync("Test", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new CreateClientDto("C1", "Test", null, null, null, null, null);

        Func<Task> act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        Func<Task> act = () => _service.DeleteAsync(999);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
