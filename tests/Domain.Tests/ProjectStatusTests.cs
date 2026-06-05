using FluentAssertions;
using Planova.Domain.ValueObjects;

namespace Domain.Tests;

public class ProjectStatusTests
{
    [Fact]
    public void FromString_ValidStatus_ReturnsStatus()
    {
        ProjectStatus.FromString("Draft").Should().Be(ProjectStatus.Draft);
        ProjectStatus.FromString("Under Review").Should().Be(ProjectStatus.UnderReview);
        ProjectStatus.FromString("In Progress").Should().Be(ProjectStatus.InProgress);
    }

    [Fact]
    public void FromString_InvalidStatus_Throws()
    {
        Action act = () => ProjectStatus.FromString("Invalid");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Draft_CanTransitionTo_UnderReview()
    {
        ProjectStatus.Draft.CanTransitionTo(ProjectStatus.UnderReview).Should().BeTrue();
    }

    [Fact]
    public void Draft_CanTransitionTo_Cancelled()
    {
        ProjectStatus.Draft.CanTransitionTo(ProjectStatus.Cancelled).Should().BeTrue();
    }

    [Fact]
    public void Draft_CannotTransitionTo_Approved()
    {
        ProjectStatus.Draft.CanTransitionTo(ProjectStatus.Approved).Should().BeFalse();
    }

    [Fact]
    public void Completed_IsTerminal()
    {
        ProjectStatus.Completed.AllowedNext().Should().BeEmpty();
    }

    [Fact]
    public void Cancelled_IsTerminal()
    {
        ProjectStatus.Cancelled.AllowedNext().Should().BeEmpty();
    }

    [Fact]
    public void InProgress_CanTransitionTo_OnHold_Completed_Cancelled()
    {
        var allowed = ProjectStatus.InProgress.AllowedNext();
        allowed.Should().Contain(ProjectStatus.OnHold);
        allowed.Should().Contain(ProjectStatus.Completed);
        allowed.Should().Contain(ProjectStatus.Cancelled);
    }

    [Fact]
    public void All_ReturnsAllStatuses()
    {
        ProjectStatus.All.Should().HaveCount(7);
    }

    [Fact]
    public void Equality_WorksByValue()
    {
        var a = ProjectStatus.FromString("Draft");
        var b = ProjectStatus.Draft;
        a.Should().Be(b);
    }
}
