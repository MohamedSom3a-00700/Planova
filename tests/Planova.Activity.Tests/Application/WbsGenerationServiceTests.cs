using FluentAssertions;
using Moq;
using Planova.Activity.Application.Services;
using Planova.Activity.Domain.Interfaces;

namespace Planova.Activity.Tests.Application;

public class WbsGenerationServiceTests
{
    private readonly Mock<IActivityService> _activityService;
    private readonly Mock<IActivityBankService> _bankService;
    private readonly Mock<IActivityRepository> _activityRepo;
    private readonly WbsGenerationService _service;

    public WbsGenerationServiceTests()
    {
        _activityService = new Mock<IActivityService>();
        _bankService = new Mock<IActivityBankService>();
        _activityRepo = new Mock<IActivityRepository>();
        _service = new WbsGenerationService(_activityService.Object, _bankService.Object, _activityRepo.Object);
    }

    [Fact]
    public async Task PreviewSimpleGenerationAsync_SingleItem_ShouldReturnOnePreview()
    {
        var wbsItemIds = new List<Guid> { Guid.NewGuid() };

        var result = await _service.PreviewSimpleGenerationAsync(wbsItemIds);

        result.Should().NotBeNull();
        result.TotalActivities.Should().Be(1);
        result.Previews.Should().HaveCount(1);
        result.Previews[0].IsNew.Should().BeTrue();
    }

    [Fact]
    public async Task PreviewSimpleGenerationAsync_MultipleItems_ShouldReturnCorrectCount()
    {
        var wbsItemIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var result = await _service.PreviewSimpleGenerationAsync(wbsItemIds);

        result.TotalActivities.Should().Be(3);
    }
}
