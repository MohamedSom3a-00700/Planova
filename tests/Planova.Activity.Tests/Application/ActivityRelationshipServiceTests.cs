using FluentAssertions;
using Moq;
using Planova.Activity.Application.Dto;
using Planova.Activity.Application.Services;
using Planova.Activity.Domain.Interfaces;

using ActivityEntity = Planova.Activity.Domain.Entities.Activity;

namespace Planova.Activity.Tests.Application;

public class ActivityRelationshipServiceTests
{
    private readonly Mock<IActivityRelationshipRepository> _repo;
    private readonly Mock<IActivityRepository> _activityRepo;
    private readonly ActivityRelationshipService _service;

    public ActivityRelationshipServiceTests()
    {
        _repo = new Mock<IActivityRelationshipRepository>();
        _activityRepo = new Mock<IActivityRepository>();
        _service = new ActivityRelationshipService(_repo.Object, _activityRepo.Object);
    }

    [Fact]
    public async Task CreateAsync_SameActivity_ShouldReject()
    {
        var id = Guid.NewGuid();

        var request = new CreateRelationshipRequest
        {
            ProjectId = 1,
            PredecessorId = id,
            SuccessorId = id,
            Type = "FS"
        };

        await FluentActions.Awaiting(() => _service.CreateAsync(request))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_ActivitiesNotInSameProject_ShouldReject()
    {
        var predId = Guid.NewGuid();
        var succId = Guid.NewGuid();

        _activityRepo.Setup(r => r.GetByIdAsync(predId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActivityEntity { Id = predId, ProjectId = 1 });
        _activityRepo.Setup(r => r.GetByIdAsync(succId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActivityEntity { Id = succId, ProjectId = 2 });

        var request = new CreateRelationshipRequest
        {
            ProjectId = 1,
            PredecessorId = predId,
            SuccessorId = succId,
            Type = "FS"
        };

        await FluentActions.Awaiting(() => _service.CreateAsync(request))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}
