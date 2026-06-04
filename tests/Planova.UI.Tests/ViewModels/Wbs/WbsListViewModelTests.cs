using FluentAssertions;
using Moq;
using Planova.Boq.Domain.Interfaces;
using Planova.UI.ViewModels.Wbs;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Enums;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.UI.Tests.ViewModels.Wbs;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;

public class WbsListViewModelTests
{
    private readonly Mock<IWbsService> _wbsService = new();
    private readonly Mock<IWbsItemRepository> _itemRepo = new();
    private readonly Mock<IBoqSession> _session = new();
    private readonly WbsListViewModel _sut;

    public WbsListViewModelTests()
    {
        _session.Setup(s => s.CurrentProjectId).Returns(Guid.NewGuid());
        _sut = new WbsListViewModel(_wbsService.Object, _itemRepo.Object, _session.Object);
    }

    [Fact]
    public async Task LoadWbsListAsync_PopulatesItems()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var expected = new List<WbsEntity>
        {
            new() { Id = id1, Name = "WBS 1" },
            new() { Id = id2, Name = "WBS 2" }
        };
        var projectHash = _session.Object.CurrentProjectId!.Value.GetHashCode();
        _wbsService.Setup(s => s.GetByProjectAsync(projectHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        _itemRepo.Setup(r => r.GetByWbsIdAsync(id1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WbsItem>());
        _itemRepo.Setup(r => r.GetByWbsIdAsync(id2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WbsItem>());

        await _sut.LoadWbsListCommand.ExecuteAsync(null);

        _sut.WbsItems.Should().HaveCount(2);
        _sut.WbsItems[0].Name.Should().Be("WBS 1");
    }

    [Fact]
    public async Task CreateWbsAsync_AddsAndReloads()
    {
        var projectHash = _session.Object.CurrentProjectId!.Value.GetHashCode();
        _wbsService.Setup(s => s.CreateAsync("New WBS", projectHash, WbsSource.Manual, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WbsEntity { Id = Guid.NewGuid(), Name = "New WBS" });
        _wbsService.Setup(s => s.GetByProjectAsync(projectHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WbsEntity>());

        await _sut.CreateWbsCommand.ExecuteAsync(null);

        _wbsService.Verify(s => s.CreateAsync("New WBS", projectHash, WbsSource.Manual, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void InitialState_IsEmpty()
    {
        _sut.WbsItems.Should().BeEmpty();
        _sut.SearchText.Should().BeEmpty();
        _sut.StatusFilter.Should().BeNull();
        _sut.SourceFilter.Should().BeNull();
    }
}
