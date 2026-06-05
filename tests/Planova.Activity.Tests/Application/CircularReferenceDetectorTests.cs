using FluentAssertions;
using Planova.Activity.Application.Services;

namespace Planova.Activity.Tests.Application;

public class CircularReferenceDetectorTests
{
    private readonly CircularReferenceDetector _detector;

    public CircularReferenceDetectorTests()
    {
        _detector = new CircularReferenceDetector();
    }

    [Fact]
    public void Detect_SelfReference_ShouldReturnCycle()
    {
        var id = Guid.NewGuid();
        var result = _detector.Detect(id, id, _ => Task.FromResult(new List<Guid>()));

        result.HasCycle.Should().BeTrue();
        result.Message.Should().Contain("self-reference");
    }

    [Fact]
    public void Detect_EmptyGraph_ShouldReturnNoCycle()
    {
        var result = _detector.Detect(Guid.NewGuid(), Guid.NewGuid(), _ => Task.FromResult(new List<Guid>()));

        result.Should().NotBeNull();
        result.HasCycle.Should().BeFalse();
    }

    [Fact]
    public void Detect_SimpleChain_ShouldReturnNoCycle()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        var d = Guid.NewGuid();

        Func<Guid, Task<List<Guid>>> getPredecessors = async (id) =>
        {
            await Task.CompletedTask;
            if (id == a) return [b];
            if (id == b) return [c];
            if (id == c) return [d];
            return [];
        };

        var result = _detector.Detect(a, d, getPredecessors);

        result.HasCycle.Should().BeFalse();
        result.CycleActivities.Should().BeEmpty();
    }

    [Fact]
    public void Detect_SimpleCycle_ShouldReturnCycle()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        Func<Guid, Task<List<Guid>>> getPredecessors = async (id) =>
        {
            await Task.CompletedTask;
            if (id == b) return [a];
            return [];
        };

        var result = _detector.Detect(a, b, getPredecessors);

        result.HasCycle.Should().BeTrue();
        result.CycleActivities.Should().NotBeEmpty();
    }

    [Fact]
    public void Detect_ComplexCycle_ShouldReturnCycle()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        var d = Guid.NewGuid();
        var e = Guid.NewGuid();

        Func<Guid, Task<List<Guid>>> getPredecessors = async (id) =>
        {
            await Task.CompletedTask;
            if (id == a) return [b];
            if (id == b) return [c];
            if (id == c) return [d];
            if (id == d) return [e];
            if (id == e) return [a];
            return [];
        };

        var result = _detector.Detect(a, e, getPredecessors);

        result.HasCycle.Should().BeTrue();
    }
}
