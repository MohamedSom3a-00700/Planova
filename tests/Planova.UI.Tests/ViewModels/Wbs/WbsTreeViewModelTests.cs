using FluentAssertions;
using Moq;
using Planova.Boq.Domain.Interfaces;
using Planova.UI.ViewModels.Wbs;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.UI.Tests.ViewModels.Wbs;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;

public class WbsTreeViewModelTests
{
    private readonly Mock<IWbsService> _wbsService = new();
    private readonly Mock<IBoqSession> _session = new();
    private readonly WbsTreeViewModel _sut;

    public WbsTreeViewModelTests()
    {
        _session.Setup(s => s.CurrentProjectId).Returns((Guid?)Guid.NewGuid());
        _wbsService.Setup(s => s.GetByProjectAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<WbsEntity>)new List<WbsEntity>());
        _sut = new WbsTreeViewModel(_wbsService.Object, _session.Object);
    }

    [Fact]
    public async Task LoadTreeAsync_BuildsRootNodes()
    {
        var wbsId = Guid.NewGuid();
        _sut.SelectedWbs = new WbsEntity { Id = wbsId, Name = "Test" };
        var items = new List<WbsItem>
        {
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Name = "Root 1", Level = 0, ParentId = null, SortOrder = 0 },
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Name = "Root 2", Level = 0, ParentId = null, SortOrder = 1 }
        };
        _wbsService.Setup(s => s.GetTreeAsync(wbsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        await _sut.LoadTreeCommand.ExecuteAsync(null);

        _sut.RootNodes.Should().HaveCount(2);
        _sut.RootNodes[0].Name.Should().Be("Root 1");
    }

    [Fact]
    public void InitialState_IsEmpty()
    {
        _sut.RootNodes.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadTreeAsync_BuildsChildren()
    {
        var wbsId = Guid.NewGuid();
        _sut.SelectedWbs = new WbsEntity { Id = wbsId, Name = "Test" };
        var parentId = Guid.NewGuid();
        var items = new List<WbsItem>
        {
            new() { Id = parentId, WbsId = wbsId, Name = "Parent", Level = 0, ParentId = null, SortOrder = 0 },
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Name = "Child", Level = 1, ParentId = parentId, SortOrder = 0 }
        };
        _wbsService.Setup(s => s.GetTreeAsync(wbsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        await _sut.LoadTreeCommand.ExecuteAsync(null);

        _sut.RootNodes.Should().ContainSingle();
        _sut.RootNodes[0].Children.Should().ContainSingle();
        _sut.RootNodes[0].Children[0].Name.Should().Be("Child");
    }

    [Fact]
    public async Task LoadTreeAsync_EmptyTree_NoNodes()
    {
        var wbsId = Guid.NewGuid();
        _sut.SelectedWbs = new WbsEntity { Id = wbsId, Name = "Test" };
        _wbsService.Setup(s => s.GetTreeAsync(wbsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WbsItem>());

        await _sut.LoadTreeCommand.ExecuteAsync(null);

        _sut.RootNodes.Should().BeEmpty();
    }
}
