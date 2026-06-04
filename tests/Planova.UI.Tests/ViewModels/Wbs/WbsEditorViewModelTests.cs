using FluentAssertions;
using Moq;
using Planova.Boq.Domain.Interfaces;
using Planova.UI.ViewModels.Wbs;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Enums;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.UI.Tests.ViewModels.Wbs;

public class WbsEditorViewModelTests
{
    private readonly Mock<IWbsService> _wbsService = new();
    private readonly Mock<IWbsItemRepository> _itemRepo = new();
    private readonly Mock<IWbsValidationService> _validation = new();
    private readonly Mock<IWbsTemplateService> _templateService = new();
    private readonly Mock<IBoqSession> _session = new();
    private readonly WbsEditorViewModel _sut;

    public WbsEditorViewModelTests()
    {
        _sut = new WbsEditorViewModel(_wbsService.Object, _itemRepo.Object, _validation.Object, _templateService.Object, _session.Object);
    }

    [Fact]
    public async Task AddChildAsync_CreatesAndReloads()
    {
        var wbsId = Guid.NewGuid();
        _sut.WbsId = wbsId;
        _sut.SelectedItem = new WbsItem { Id = Guid.NewGuid(), WbsLevel = WbsLevelType.ControlAccount };

        _itemRepo.Setup(r => r.AddAsync(It.IsAny<WbsItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WbsItem i, CancellationToken _) => i);
        _wbsService.Setup(s => s.RedistributeWeightsAsync(wbsId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _itemRepo.Setup(r => r.GetByWbsIdAsync(wbsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WbsItem>());

        await _sut.AddChildCommand.ExecuteAsync(null);

        _itemRepo.Verify(r => r.AddAsync(It.IsAny<WbsItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddChildAsync_WorkPackageParent_DoesNotAdd()
    {
        _sut.SelectedItem = new WbsItem { Id = Guid.NewGuid(), WbsLevel = WbsLevelType.WorkPackage };

        await _sut.AddChildCommand.ExecuteAsync(null);

        _itemRepo.Verify(r => r.AddAsync(It.IsAny<WbsItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteItemAsync_SelectedNull_DoesNothing()
    {
        await _sut.DeleteItemCommand.ExecuteAsync(null);

        _itemRepo.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteItemAsync_DeletesWithDescendants()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var wbsId = Guid.NewGuid();
        _sut.WbsId = wbsId;
        _sut.SelectedItem = new WbsItem { Id = parentId, Name = "Parent" };
        var allItems = new List<WbsItem>
        {
            new() { Id = parentId, WbsId = wbsId, Name = "Parent" },
            new() { Id = childId, WbsId = wbsId, Name = "Child", ParentId = parentId }
        };
        _itemRepo.Setup(r => r.GetByWbsIdAsync(wbsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allItems);

        await _sut.DeleteItemCommand.ExecuteAsync(null);

        _itemRepo.Verify(r => r.DeleteRangeAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(parentId) && ids.Contains(childId)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BulkUpdateAsync_SetsFields()
    {
        var item1 = new WbsItem { Id = Guid.NewGuid(), Name = "Item 1" };
        var item2 = new WbsItem { Id = Guid.NewGuid(), Name = "Item 2" };
        _sut.SelectedItems.Add(item1);
        _sut.SelectedItems.Add(item2);
        var date = DateTime.Today;
        _sut.BulkPlannedStart = date;
        _sut.BulkAssignedTo = "Engineer";

        _itemRepo.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<WbsItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _itemRepo.Setup(r => r.GetByWbsIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WbsItem>());

        await _sut.BulkUpdateCommand.ExecuteAsync(null);

        item1.PlannedStart.Should().Be(date);
        item1.AssignedTo.Should().Be("Engineer");
        item2.PlannedStart.Should().Be(date);
        item2.AssignedTo.Should().Be("Engineer");
    }

    [Fact]
    public void InitialState_IsEmpty()
    {
        _sut.Items.Should().BeEmpty();
        _sut.SelectedItems.Should().BeEmpty();
        _sut.SelectedItem.Should().BeNull();
        _sut.IsBulkEdit.Should().BeFalse();
    }
}
