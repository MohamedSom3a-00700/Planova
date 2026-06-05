using FluentAssertions;
using Moq;
using Planova.Activity.Application.Dto;
using Planova.Activity.Application.Services;
using Planova.Activity.Domain.Enums;
using Planova.Activity.Domain.Interfaces;

using ActivityEntity = Planova.Activity.Domain.Entities.Activity;

namespace Planova.Activity.Tests.Application;

public class ActivityServiceTests
{
    private readonly Mock<IActivityRepository> _repo;
    private readonly Mock<IActivityRelationshipRepository> _relRepo;
    private readonly ActivityService _service;

    public ActivityServiceTests()
    {
        _repo = new Mock<IActivityRepository>();
        _relRepo = new Mock<IActivityRelationshipRepository>();
        _service = new ActivityService(_repo.Object, _relRepo.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateCode()
    {
        _repo.Setup(r => r.GetNextCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("A-001");

        var request = new CreateActivityRequest
        {
            ProjectId = 1,
            Name = "Test",
            ActivityType = "Task",
            Duration = 5
        };

        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be("A-001");
        result.Status.Should().Be("NotStarted");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActivityEntity?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ChangeStatusAsync_InvalidTransition_ShouldThrow()
    {
        var activity = new ActivityEntity
        {
            Id = Guid.NewGuid(),
            Status = ActivityStatus.NotStarted
        };

        _repo.Setup(r => r.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        await FluentActions.Awaiting(() => _service.ChangeStatusAsync(activity.Id, ActivityStatus.Completed))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}
