using FluentAssertions;
using Moq;
using Planova.Application.Repositories;
using Planova.Application.Services;
using Planova.Domain.Entities;

namespace Application.Tests;

public class ReportServiceTests
{
    [Fact]
    public async Task GetProjectSummary_EmptyRepo_ReturnsEmpty()
    {
        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Project>());

        var service = new ProjectService(projectRepo.Object, Mock.Of<IClientRepository>(), Mock.Of<IContractRepository>(), Mock.Of<IContractorRepository>(), Mock.Of<ISubcontractorRepository>());
        var result = await service.GetAllAsync();

        result.Should().BeEmpty();
    }
}
