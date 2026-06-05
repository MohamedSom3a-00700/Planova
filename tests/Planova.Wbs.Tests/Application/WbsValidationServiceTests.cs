using FluentAssertions;
using Planova.Wbs.Application.Services;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Enums;
using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;

namespace Planova.Wbs.Tests.Application;

public class WbsValidationServiceTests
{
    private readonly WbsValidationService _sut = new();

    [Fact]
    public async Task ValidateWbsAsync_EmptyName_ReturnsError()
    {
        var wbs = new WbsEntity
        {
            Id = Guid.NewGuid(),
            Name = "",
            ProjectId = 1
        };

        var errors = await _sut.ValidateWbsAsync(wbs, CancellationToken.None);

        errors.Should().Contain(e => e.Property == "Name");
    }

    [Fact]
    public async Task ValidateWbsAsync_ValidName_ReturnsNoErrors()
    {
        var wbs = new WbsEntity
        {
            Id = Guid.NewGuid(),
            Name = "Valid WBS",
            ProjectId = 1
        };

        var errors = await _sut.ValidateWbsAsync(wbs, CancellationToken.None);

        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateWbsAsync_NameTooLong_ReturnsError()
    {
        var wbs = new WbsEntity
        {
            Id = Guid.NewGuid(),
            Name = new string('A', 201),
            ProjectId = 1
        };

        var errors = await _sut.ValidateWbsAsync(wbs, CancellationToken.None);

        errors.Should().Contain(e => e.Property == "Name");
    }

    [Fact]
    public async Task ValidateItemAsync_EmptyName_ReturnsError()
    {
        var item = new WbsItem { Name = "" };

        var errors = await _sut.ValidateItemAsync(item, [], CancellationToken.None);

        errors.Should().Contain(e => e.Property == "Name");
    }

    [Fact]
    public async Task ValidateItemAsync_ValidItem_ReturnsNoErrors()
    {
        var item = new WbsItem
        {
            Id = Guid.NewGuid(),
            Name = "Foundation",
            Weight = 25
        };

        var errors = await _sut.ValidateItemAsync(item, [], CancellationToken.None);

        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateItemAsync_WeightExceeds100_ReturnsError()
    {
        var item = new WbsItem
        {
            Id = Guid.NewGuid(),
            Name = "Too Heavy",
            Weight = 60
        };
        var siblings = new List<WbsItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Sibling", Weight = 50 }
        };

        var errors = await _sut.ValidateItemAsync(item, siblings, CancellationToken.None);

        errors.Should().Contain(e => e.Property == "Weight");
    }

    [Fact]
    public async Task ValidateItemAsync_FinishBeforeStart_ReturnsError()
    {
        var item = new WbsItem
        {
            Id = Guid.NewGuid(),
            Name = "Bad Dates",
            PlannedStart = DateTime.Today.AddDays(5),
            PlannedFinish = DateTime.Today
        };

        var errors = await _sut.ValidateItemAsync(item, [], CancellationToken.None);

        errors.Should().Contain(e => e.Property == "PlannedFinish");
    }

    [Fact]
    public async Task ValidateItemAsync_WorkPackageWithChildren_ReturnsError()
    {
        var item = new WbsItem
        {
            Id = Guid.NewGuid(),
            Name = "WorkPackage",
            WbsLevel = WbsLevelType.WorkPackage
        };
        var siblings = new List<WbsItem>
        {
            new() { Id = Guid.NewGuid(), ParentId = item.Id, Name = "Child" }
        };

        var errors = await _sut.ValidateItemAsync(item, siblings, CancellationToken.None);

        errors.Should().Contain(e => e.Property == "WbsLevel");
    }

    [Fact]
    public void IsCircularReference_SelfParent_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var items = new List<WbsItem>
        {
            new() { Id = id }
        };

        var result = _sut.IsCircularReference(id, id, items);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsCircularReference_NullParent_ReturnsTrue()
    {
        var id = Guid.NewGuid();

        var result = _sut.IsCircularReference(id, null, []);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsCircularReference_ValidParent_ReturnsFalse()
    {
        var childId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var items = new List<WbsItem>
        {
            new() { Id = childId, ParentId = parentId },
            new() { Id = parentId }
        };

        var result = _sut.IsCircularReference(childId, parentId, items);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsCircularReference_DeepCycle_ReturnsTrue()
    {
        var rootId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var grandchildId = Guid.NewGuid();
        var items = new List<WbsItem>
        {
            new() { Id = rootId },
            new() { Id = childId, ParentId = rootId },
            new() { Id = grandchildId, ParentId = childId }
        };

        var result = _sut.IsCircularReference(rootId, grandchildId, items);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateWeightConsistencyAsync_ValidWeights_ReturnsTrue()
    {
        var wbsId = Guid.NewGuid();
        var items = new List<WbsItem>
        {
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Weight = 30 },
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Weight = 30 },
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Weight = 40 }
        };

        var result = await _sut.ValidateWeightConsistencyAsync(wbsId, items, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateWeightConsistencyAsync_Over100_ReturnsFalse()
    {
        var wbsId = Guid.NewGuid();
        var items = new List<WbsItem>
        {
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Weight = 60 },
            new() { Id = Guid.NewGuid(), WbsId = wbsId, Weight = 50 }
        };

        var result = await _sut.ValidateWeightConsistencyAsync(wbsId, items, CancellationToken.None);

        result.Should().BeFalse();
    }
}
