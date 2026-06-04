using FluentAssertions;
using Moq;
using Planova.Wbs.Application.Services;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Enums;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Tests.Services;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;

public class WbsServiceTests
{
    private readonly Mock<IWbsRepository> _wbsRepo = new();
    private readonly Mock<IWbsItemRepository> _itemRepo = new();
    private readonly Mock<IWbsValidationService> _validationService = new();
    private readonly WbsService _sut;

    public WbsServiceTests()
    {
        _sut = new WbsService(_wbsRepo.Object, _itemRepo.Object, _validationService.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidData_CreatesWbs()
    {
        _validationService.Setup(v => v.ValidateWbsAsync(It.IsAny<WbsEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _wbsRepo.Setup(r => r.AddAsync(It.IsAny<WbsEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WbsEntity w, CancellationToken _) => w);

        var result = await _sut.CreateAsync("Test Project", 1, WbsSource.Manual, null, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Project");
        result.ProjectId.Should().Be(1);
        result.Source.Should().Be(WbsSource.Manual);
        result.Status.Should().Be(WbsStatus.Draft);
    }

    [Fact]
    public async Task CreateAsync_InvalidData_ThrowsException()
    {
        _validationService.Setup(v => v.ValidateWbsAsync(It.IsAny<WbsEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ValidationError("Name", "WBS name is required")]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync("", 1, WbsSource.Manual, null, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsWbs()
    {
        var id = Guid.NewGuid();
        var expected = new WbsEntity { Id = id, Name = "Test", ProjectId = 1 };
        _wbsRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetByIdAsync(id, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsProjectWbss()
    {
        var wbss = new List<WbsEntity>
        {
            new() { Id = Guid.NewGuid(), ProjectId = 1, Name = "WBS 1" },
            new() { Id = Guid.NewGuid(), ProjectId = 1, Name = "WBS 2" }
        };
        _wbsRepo.Setup(r => r.GetByProjectIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wbss);

        var result = await _sut.GetByProjectAsync(1, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ValidData_UpdatesWbs()
    {
        var wbs = new WbsEntity { Id = Guid.NewGuid(), Name = "Updated", ProjectId = 1 };
        _validationService.Setup(v => v.ValidateWbsAsync(It.IsAny<WbsEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _wbsRepo.Setup(r => r.UpdateAsync(It.IsAny<WbsEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(wbs);

        var result = await _sut.UpdateAsync(wbs, CancellationToken.None);

        result.Should().Be(wbs);
    }

    [Fact]
    public async Task DeleteAsync_DeletesWbs()
    {
        var id = Guid.NewGuid();
        _wbsRepo.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.DeleteAsync(id, CancellationToken.None);

        _wbsRepo.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeStatusAsync_ValidTransition_ChangesStatus()
    {
        var id = Guid.NewGuid();
        var wbs = new WbsEntity { Id = id, Status = WbsStatus.Draft, Revision = 0 };
        _wbsRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wbs);

        await _sut.ChangeStatusAsync(id, WbsStatus.Final, CancellationToken.None);

        wbs.Status.Should().Be(WbsStatus.Final);
        wbs.Revision.Should().Be(1);
    }

    [Fact]
    public async Task ChangeStatusAsync_InvalidTransition_ThrowsException()
    {
        var id = Guid.NewGuid();
        var wbs = new WbsEntity { Id = id, Status = WbsStatus.Draft };
        _wbsRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wbs);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ChangeStatusAsync(id, WbsStatus.Approved, CancellationToken.None));
    }

    [Fact]
    public async Task GetTreeAsync_ReturnsItems()
    {
        var wbsId = Guid.NewGuid();
        var items = new List<WbsItem>
        {
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Name = "Item 1" },
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Name = "Item 2" }
        };
        _itemRepo.Setup(r => r.GetByWbsIdAsync(wbsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var result = await _sut.GetTreeAsync(wbsId, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task RedistributeWeightsAsync_EqualDistribution()
    {
        var wbsId = Guid.NewGuid();
        var items = new List<WbsItem>
        {
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Weight = 10 },
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Weight = 20 },
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Weight = 30 }
        };
        _itemRepo.Setup(r => r.GetByWbsIdAsync(wbsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);
        _itemRepo.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<WbsItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.RedistributeWeightsAsync(wbsId, CancellationToken.None);

        items.Sum(i => i.Weight).Should().Be(100m);
        items[0].Weight.Should().Be(33.33m);
        items[1].Weight.Should().Be(33.33m);
        items[2].Weight.Should().Be(33.34m);
    }
}
