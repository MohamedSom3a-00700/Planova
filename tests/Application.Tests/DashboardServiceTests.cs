using FluentAssertions;
using Moq;
using Planova.Application.Repositories;
using Planova.Application.Services;
using Planova.Domain.Entities;

namespace Application.Tests;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetDashboardSummaryAsync_ReturnsCount()
    {
        var projectRepo = new Mock<IProjectRepository>();
        var clientRepo = new Mock<IClientRepository>();
        var contractRepo = new Mock<IContractRepository>();

        projectRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Project>());
        clientRepo.Setup(r => r.GetCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        contractRepo.Setup(r => r.GetCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var service = new ProjectService(projectRepo.Object, clientRepo.Object, contractRepo.Object, Mock.Of<IContractorRepository>(), Mock.Of<ISubcontractorRepository>());
        var result = await service.GetDashboardSummaryAsync();

        result.TotalClients.Should().Be(5);
        result.TotalContracts.Should().Be(3);
    }
}
