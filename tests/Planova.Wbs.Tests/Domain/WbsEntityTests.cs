using FluentAssertions;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Enums;
using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;

namespace Planova.Wbs.Tests.Domain;

public class WbsEntityTests
{
    [Fact]
    public void CreateWbs_SetsDefaultValues()
    {
        var wbs = new WbsEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            Name = "Test WBS",
            Source = WbsSource.Manual,
            Status = WbsStatus.Draft,
            Revision = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        wbs.Id.Should().NotBeEmpty();
        wbs.ProjectId.Should().Be(1);
        wbs.Name.Should().Be("Test WBS");
        wbs.Source.Should().Be(WbsSource.Manual);
        wbs.Status.Should().Be(WbsStatus.Draft);
        wbs.Revision.Should().Be(0);
    }

    [Fact]
    public void WbsItem_CreatesWithHierarchy()
    {
        var parent = new WbsItem
        {
            Id = Guid.NewGuid(),
            WbsId = Guid.NewGuid(),
            Name = "Parent",
            Level = 0,
            WbsLevel = WbsLevelType.ControlAccount
        };

        var child = new WbsItem
        {
            Id = Guid.NewGuid(),
            WbsId = parent.WbsId,
            ParentId = parent.Id,
            Name = "Child",
            Level = 1,
            WbsLevel = WbsLevelType.WorkPackage
        };

        child.ParentId.Should().Be(parent.Id);
        child.Level.Should().Be(1);
    }

    [Fact]
    public void WbsItem_DefaultWeightIsNull()
    {
        var item = new WbsItem
        {
            Id = Guid.NewGuid(),
            WbsId = Guid.NewGuid(),
            Name = "Test"
        };

        item.Weight.Should().BeNull();
    }

    [Fact]
    public void WbsItem_CanSetAndGetWeight()
    {
        var item = new WbsItem
        {
            Id = Guid.NewGuid(),
            WbsId = Guid.NewGuid(),
            Name = "Test",
            Weight = 33.5m
        };

        item.Weight.Should().Be(33.5m);
    }

    [Fact]
    public void WbsTemplateItem_HasDefaultDuration()
    {
        var template = new WbsTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Building Construction",
            Category = "Construction",
            IsStandard = true
        };

        var item = new WbsTemplateItem
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Name = "Foundation",
            Level = 1,
            DefaultDurationDays = 30,
            TypicalWeight = 10
        };

        item.DefaultDurationDays.Should().Be(30);
        item.TypicalWeight.Should().Be(10);
    }
}
