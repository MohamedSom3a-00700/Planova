using FluentAssertions;
using Moq;
using Planova.Activity.Application.Dto;
using Planova.Activity.Application.Services;
using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Enums;
using Planova.Activity.Domain.Interfaces;

namespace Planova.Activity.Tests.Application;

public class ActivityBankServiceTests
{
    private readonly Mock<IActivityBankRepository> _bankRepo;
    private readonly Mock<IActivityBankItemRepository> _itemRepo;
    private readonly Mock<IActivityBankItemRelationshipRepository> _relRepo;
    private readonly Mock<IActivityRepository> _activityRepo;
    private readonly ActivityBankService _service;

    public ActivityBankServiceTests()
    {
        _bankRepo = new Mock<IActivityBankRepository>();
        _itemRepo = new Mock<IActivityBankItemRepository>();
        _relRepo = new Mock<IActivityBankItemRelationshipRepository>();
        _activityRepo = new Mock<IActivityRepository>();
        _service = new ActivityBankService(_bankRepo.Object, _itemRepo.Object, _relRepo.Object, _activityRepo.Object);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ShouldReturnDto()
    {
        var id = Guid.NewGuid();

        _bankRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActivityBank
            {
                Id = id,
                Category = "Concrete",
                Code = "TEST-001",
                Name = "Test",
                IsStandard = true,
                Version = 1,
                Tags = "[]"
            });

        var result = await _service.GetByIdAsync(id);

        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.IsStandard.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ShouldThrow()
    {
        _bankRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActivityBank?)null);

        await FluentActions.Awaiting(() => _service.GetByIdAsync(Guid.NewGuid()))
            .Should().ThrowAsync<KeyNotFoundException>();
    }
}
