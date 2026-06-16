using FluentAssertions;
using NSubstitute;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Application.Models;
using Planova.Primavera.Domain.Interfaces;
using Planova.ScheduleComparison.Application.Services;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Enums;
using Planova.ScheduleComparison.Domain.Interfaces;

namespace Planova.ScheduleComparison.Tests.Application.Services;

public class ScheduleComparisonServiceTests
{
    private readonly IComparisonRepository _repository;
    private readonly ScheduleComparisonService _service;

    public ScheduleComparisonServiceTests()
    {
        _repository = Substitute.For<IComparisonRepository>();
        _service = new ScheduleComparisonService(
            _repository,
            Substitute.For<IServiceProvider>(),
            default!);
    }

    [Fact]
    public async Task ListSessionsAsync_ReturnsSessionsFromRepository()
    {
        var projectId = 1;
        var expected = new List<ComparisonSession>
        {
            new() { Id = Guid.NewGuid(), ProjectId = projectId },
            new() { Id = Guid.NewGuid(), ProjectId = projectId },
        };

        _repository.GetSessionsByProjectAsync(projectId, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _service.ListSessionsAsync(projectId);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetSessionAsync_ReturnsSessionById()
    {
        var sessionId = Guid.NewGuid();
        var expected = new ComparisonSession { Id = sessionId };

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _service.GetSessionAsync(sessionId);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task ReOpenSessionAsync_WhenCompleted_Succeeds()
    {
        var sessionId = Guid.NewGuid();
        var session = new ComparisonSession { Id = sessionId, State = SessionState.Completed };

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(session);

        Func<Task> act = () => _service.ReOpenSessionAsync(sessionId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ReOpenSessionAsync_WhenNotCompleted_ThrowsInvalidOperationException()
    {
        var sessionId = Guid.NewGuid();
        var session = new ComparisonSession { Id = sessionId, State = SessionState.Draft };

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(session);

        Func<Task> act = () => _service.ReOpenSessionAsync(sessionId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{sessionId}*");
    }

    [Fact]
    public async Task ReOpenSessionAsync_WhenSessionNotFound_ThrowsInvalidOperationException()
    {
        var sessionId = Guid.NewGuid();

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns((ComparisonSession?)null);

        Func<Task> act = () => _service.ReOpenSessionAsync(sessionId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{sessionId}*");
    }

    [Fact]
    public async Task SoftDeleteSessionAsync_SetsStateToCancelled()
    {
        var sessionId = Guid.NewGuid();
        var session = new ComparisonSession { Id = sessionId, State = SessionState.Completed };

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(session);

        await _service.SoftDeleteSessionAsync(sessionId);

        session.State.Should().Be(SessionState.Cancelled);
        _ = _repository.Received(1).UpdateSessionAsync(session, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SoftDeleteSessionAsync_WhenSessionNotFound_ThrowsInvalidOperationException()
    {
        var sessionId = Guid.NewGuid();

        _repository.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns((ComparisonSession?)null);

        Func<Task> act = () => _service.SoftDeleteSessionAsync(sessionId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{sessionId}*");
    }

    [Fact]
    public async Task CompareAsync_WithPrimaveraSourceAndTarget_CompletesSuccessfully()
    {
        var repository = Substitute.For<IComparisonRepository>();
        var primaveraService = Substitute.For<IPrimaveraWorkspaceService>();
        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(typeof(IPrimaveraWorkspaceService)).Returns(primaveraService);

        var sourceSnapshot = new PrimaveraWorkspaceSnapshot
        {
            Activities = new List<PrimaveraActivityDto>
            {
                new() { Id = Guid.NewGuid(), TaskId = "A1", Name = "Foundation", Duration = 5 }
            },
            Relationships = new List<PrimaveraRelationshipDto>
            {
                new() { Id = Guid.NewGuid(), PredTaskId = "A1", SuccTaskId = "A2", Type = "FS", LagDuration = 0 }
            },
            ResourceAssignments = new List<PrimaveraResourceAssignmentDto>
            {
                new() { Id = Guid.NewGuid(), TaskId = "A1", ResourceId = "R1", Units = 100, CostPerUnit = 50 }
            }
        };

        var targetSnapshot = new PrimaveraWorkspaceSnapshot
        {
            Activities = new List<PrimaveraActivityDto>
            {
                new() { Id = Guid.NewGuid(), TaskId = "A1", Name = "Foundation", Duration = 7 }
            },
            Relationships = new List<PrimaveraRelationshipDto>
            {
                new() { Id = Guid.NewGuid(), PredTaskId = "A1", SuccTaskId = "A2", Type = "FS", LagDuration = 0 }
            },
            ResourceAssignments = new List<PrimaveraResourceAssignmentDto>
            {
                new() { Id = Guid.NewGuid(), TaskId = "A1", ResourceId = "R1", Units = 100, CostPerUnit = 50 }
            }
        };

        primaveraService.GetSnapshotAsync(1, Arg.Any<CancellationToken>()).Returns(
            sourceSnapshot, targetSnapshot);

        ComparisonSession? capturedSession = null;
        repository.When(x => x.AddSessionAsync(Arg.Any<ComparisonSession>(), Arg.Any<CancellationToken>()))
            .Do(x => capturedSession = x.Arg<ComparisonSession>());
        repository.When(x => x.UpdateSessionAsync(Arg.Any<ComparisonSession>(), Arg.Any<CancellationToken>()))
            .Do(x => capturedSession = x.Arg<ComparisonSession>());

        var service = new ScheduleComparisonService(repository, serviceProvider, default!);

        var scopes = new List<ComparisonScope> { ComparisonScope.Activities, ComparisonScope.Logic, ComparisonScope.Resources };
        var result = await service.CompareAsync(
            1, null, null, "Primavera", "Primavera", "Source", "Target", scopes);

        result.State.Should().Be(SessionState.Completed);
        result.Mode.Should().Be(ComparisonMode.XerVsXer);
        result.SourceKind.Should().Be("Primavera");
        result.TargetKind.Should().Be("Primavera");
    }

    [Fact]
    public async Task CompareAsync_WithPrimaveraKind_DeterminesXerVsXerMode()
    {
        var repository = Substitute.For<IComparisonRepository>();
        var primaveraService = Substitute.For<IPrimaveraWorkspaceService>();
        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(typeof(IPrimaveraWorkspaceService)).Returns(primaveraService);

        primaveraService.GetSnapshotAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PrimaveraWorkspaceSnapshot());

        var service = new ScheduleComparisonService(repository, serviceProvider, default!);

        ComparisonSession? capturedSession = null;
        repository.When(x => x.AddSessionAsync(Arg.Any<ComparisonSession>(), Arg.Any<CancellationToken>()))
            .Do(x => capturedSession = x.Arg<ComparisonSession>());
        repository.When(x => x.UpdateSessionAsync(Arg.Any<ComparisonSession>(), Arg.Any<CancellationToken>()))
            .Do(x => capturedSession = x.Arg<ComparisonSession>());

        var scopes = new List<ComparisonScope> { ComparisonScope.Activities };
        var result = await service.CompareAsync(
            1, null, null, "Primavera", "Primavera", "XER Source", "XER Target", scopes);

        result.Mode.Should().Be(ComparisonMode.XerVsXer);
    }

    [Fact]
    public async Task CompareAsync_WhenPrimaveraUnavailable_FallsThroughAndFails()
    {
        var repository = Substitute.For<IComparisonRepository>();
        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(typeof(IPrimaveraWorkspaceService)).Returns(null);

        ComparisonSession? capturedSession = null;
        repository.When(x => x.AddSessionAsync(Arg.Any<ComparisonSession>(), Arg.Any<CancellationToken>()))
            .Do(x => capturedSession = x.Arg<ComparisonSession>());
        repository.When(x => x.UpdateSessionAsync(Arg.Any<ComparisonSession>(), Arg.Any<CancellationToken>()))
            .Do(x => capturedSession = x.Arg<ComparisonSession>());

        var service = new ScheduleComparisonService(repository, serviceProvider, default!);

        var scopes = new List<ComparisonScope> { ComparisonScope.Activities };

        Func<Task> act = () => service.CompareAsync(
            1, null, null, "Primavera", "Snapshot", "XER Source", "Snapshot Target", scopes);

        await act.Should().ThrowAsync<Exception>();
        capturedSession.Should().NotBeNull();
        capturedSession!.State.Should().Be(SessionState.Failed);
    }
}
