using FluentAssertions;
using Moq;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;
using Planova.Wbs.Application.Services;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Tests.Services;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;
using WbsMappingResult = Planova.Wbs.Application.Dto.WbsMappingResult;
using MappedItem = Planova.Wbs.Application.Dto.MappedItem;

public class WbsBoqMappingServiceTests
{
    private readonly Mock<IBoqService> _boqService = new();
    private readonly Mock<IWbsRepository> _wbsRepo = new();
    private readonly Mock<IWbsItemRepository> _itemRepo = new();
    private readonly WbsBoqMappingService _sut;

    public WbsBoqMappingServiceTests()
    {
        _sut = new WbsBoqMappingService(_boqService.Object, _wbsRepo.Object, _itemRepo.Object);
    }

    [Fact]
    public async Task MapOneToOneAsync_ReturnsMappedItems()
    {
        var boqId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var boq = new BoqDto(boqId, Guid.NewGuid(), "Test BOQ", null, "USD", BoqStatus.Draft, 0, 0, null, 1, DateTime.UtcNow, DateTime.UtcNow);
        var tree = new List<BoqItemDto>
        {
            new(itemId, boqId, null, 0, "A.1", "Concrete Work", "m3", 100, 50, 5000, ItemType.Item, 0, true, null, null, null, new List<BoqItemDto>())
        };
        _boqService.Setup(s => s.GetByIdAsync(boqId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boq);
        _boqService.Setup(s => s.GetTreeAsync(boqId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tree);

        var result = await _sut.MapOneToOneAsync(boqId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().NotBeEmpty();
        result.Strategy.Should().Be("OneToOne");
    }

    [Fact]
    public async Task MapGroupedAsync_GroupsByCode()
    {
        var boqId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var boq = new BoqDto(boqId, Guid.NewGuid(), "Test BOQ", null, "USD", BoqStatus.Draft, 0, 0, null, 1, DateTime.UtcNow, DateTime.UtcNow);
        var tree = new List<BoqItemDto>
        {
            new(groupId, boqId, null, 0, "A", "Civil Works", "lump", 1, 1, 1, ItemType.Section, 0, true, null, null, null,
                new List<BoqItemDto>
                {
                    new(childId, boqId, groupId, 0, "A.1", "Excavation", "m3", 100, 50, 5000, ItemType.Item, 1, true, null, null, null, new List<BoqItemDto>())
                })
        };
        _boqService.Setup(s => s.GetByIdAsync(boqId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boq);
        _boqService.Setup(s => s.GetTreeAsync(boqId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tree);

        var result = await _sut.MapGroupedAsync(boqId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Strategy.Should().Be("Grouped");
    }

    [Fact]
    public async Task CommitMappingAsync_CreatesWbs()
    {
        _wbsRepo.Setup(r => r.AddAsync(It.IsAny<WbsEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WbsEntity w, CancellationToken _) => w);
        _itemRepo.Setup(r => r.AddAsync(It.IsAny<WbsItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WbsItem i, CancellationToken _) => i);
        _itemRepo.Setup(r => r.GetByWbsIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WbsItem>());
        _itemRepo.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<WbsItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = new WbsMappingResult(
            new List<MappedItem>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), null, "Concrete", 0, 0, "Summary"),
                new(Guid.NewGuid(), Guid.NewGuid(), null, "Steel", 0, 1, "Summary")
            },
            "OneToOne");

        var wbs = await _sut.CommitMappingAsync(result, "Test WBS", 1, CancellationToken.None);

        wbs.Should().NotBeNull();
        wbs.Name.Should().Be("Test WBS");
    }
}
