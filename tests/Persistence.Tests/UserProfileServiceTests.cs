using FluentAssertions;
using Moq;
using Planova.Application.Repositories;
using Planova.Application.Services;
using Planova.Domain.Entities;

namespace Application.Tests;

public class UserProfileServiceTests
{
    [Fact]
    public async Task GetProfileAsync_ReturnsNull_WhenNoProfile()
    {
        var repo = new Mock<IUserProfileRepository>();
        repo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserPreferences?)null);

        var service = new Planova.Persistence.Services.UserProfileService(repo.Object);
        var result = await service.GetProfileAsync();

        result.Should().BeNull();
    }
}
