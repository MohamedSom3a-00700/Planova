using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Persistence.Repositories;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Enums;

namespace Planova.ScheduleComparison.Tests.Persistence;

public class ComparisonRepositoryRoundTripTests : IDisposable
{
    private readonly PlanovaDbContext _context;
    private readonly ComparisonRepository _repo;

    public ComparisonRepositoryRoundTripTests()
    {
        var options = new DbContextOptionsBuilder<PlanovaDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new PlanovaDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repo = new ComparisonRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [Fact]
    public async Task ComparisonSession_CreateAndRetrieve_RoundTrips()
    {
        var session = new ComparisonSession
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            Mode = ComparisonMode.UpdateVsUpdate,
            State = SessionState.Draft,
            SourceKind = "Snapshot",
            TargetKind = "Snapshot",
            SourceLabel = "Baseline",
            TargetLabel = "Current",
            IncludedScopes = "Activities,Logic",
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddSessionAsync(session);
        var retrieved = await _repo.GetSessionByIdAsync(session.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(session.Id);
        retrieved.ProjectId.Should().Be(1);
        retrieved.Mode.Should().Be(ComparisonMode.UpdateVsUpdate);
        retrieved.State.Should().Be(SessionState.Draft);
    }

    [Fact]
    public async Task ComparisonSession_Update_RoundTrips()
    {
        var session = new ComparisonSession
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            Mode = ComparisonMode.UpdateVsUpdate,
            State = SessionState.Draft,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddSessionAsync(session);

        session.State = SessionState.Running;
        session.StartedAt = DateTime.UtcNow;
        await _repo.UpdateSessionAsync(session);

        var retrieved = await _repo.GetSessionByIdAsync(session.Id);
        retrieved!.State.Should().Be(SessionState.Running);
        retrieved.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ComparisonResult_PersistAndQuery_RoundTrips()
    {
        var sessionId = Guid.NewGuid();
        var session = new ComparisonSession
        {
            Id = sessionId,
            ProjectId = 1,
            Mode = ComparisonMode.UpdateVsUpdate,
            State = SessionState.Draft,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddSessionAsync(session);

        var results = new List<ComparisonResult>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                EntityType = "Activity",
                MatchKey = "A1",
                ChangeType = ChangeType.Modified,
                FieldName = "Duration",
                OldValue = "5",
                NewValue = "10",
                Severity = "Minor"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                EntityType = "Relationship",
                MatchKey = "R1",
                ChangeType = ChangeType.Added,
                Severity = "Major"
            }
        };

        await _repo.AddResultsAsync(results);

        var retrieved = await _repo.GetResultsBySessionIdAsync(sessionId);
        retrieved.Should().HaveCount(2);
        retrieved.Should().Contain(r => r.MatchKey == "A1" && r.FieldName == "Duration");
        retrieved.Should().Contain(r => r.MatchKey == "R1" && r.ChangeType == ChangeType.Added);
    }

    [Fact]
    public async Task ScheduleSnapshot_CreateAndRetrieve_RoundTrips()
    {
        var snapshot = new ScheduleSnapshot
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            Label = "Test Snapshot",
            SnapshotData = "{}",
            CapturedAt = DateTime.UtcNow,
            ActivityCount = 10,
            RelationshipCount = 5
        };

        await _repo.AddSnapshotAsync(snapshot);

        var retrieved = await _repo.GetSnapshotByIdAsync(snapshot.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Label.Should().Be("Test Snapshot");
        retrieved.ActivityCount.Should().Be(10);

        var snapshots = await _repo.GetSnapshotsByProjectAsync(1);
        snapshots.Should().Contain(s => s.Id == snapshot.Id);
    }

    [Fact]
    public async Task ScheduleSnapshot_Delete_RemovesFromDatabase()
    {
        var snapshot = new ScheduleSnapshot
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            Label = "To Delete",
            SnapshotData = "{}",
            CapturedAt = DateTime.UtcNow
        };

        await _repo.AddSnapshotAsync(snapshot);
        await _repo.DeleteSnapshotAsync(snapshot);

        var retrieved = await _repo.GetSnapshotByIdAsync(snapshot.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task ComparisonResult_PagedQuery_ReturnsCorrectPage()
    {
        var sessionId = Guid.NewGuid();
        var session = new ComparisonSession
        {
            Id = sessionId,
            ProjectId = 1,
            Mode = ComparisonMode.UpdateVsUpdate,
            State = SessionState.Draft,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddSessionAsync(session);

        var results = Enumerable.Range(1, 25).Select(i => new ComparisonResult
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            EntityType = "Activity",
            MatchKey = $"A{i}",
            ChangeType = ChangeType.Modified,
            FieldName = "Duration",
            OldValue = "5",
            NewValue = "10"
        }).ToList();

        await _repo.AddResultsAsync(results);

        var all = await _repo.GetResultsBySessionIdAsync(sessionId);

        var page1 = await _repo.GetResultsBySessionIdPagedAsync(sessionId, 0, 10);
        page1.Should().HaveCount(10);

        var page2 = await _repo.GetResultsBySessionIdPagedAsync(sessionId, 10, 10);
        page2.Should().HaveCount(10);

        var page3 = await _repo.GetResultsBySessionIdPagedAsync(sessionId, 20, 10);
        page3.Should().HaveCount(5);

        var combinedCount = page1.Count + page2.Count + page3.Count;
        combinedCount.Should().Be(all.Count);
    }

    [Fact]
    public async Task ComparisonRule_CreateAndRetrieve_RoundTrips()
    {
        var rule = new ComparisonRule
        {
            Id = Guid.NewGuid(),
            ProjectId = 1,
            Name = "Default Rules",
            EnableFuzzyMatching = true,
            MatchingStrategyPreference = "ProvenanceId,ActivityId,WbsCode",
            SeverityThresholdCritical = 10.0,
            SeverityThresholdMajor = 5.0,
            SeverityThresholdMinor = 2.0
        };

        _context.ComparisonRules.Add(rule);
        await _context.SaveChangesAsync();

        var retrieved = await _repo.GetRuleByProjectAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Default Rules");
        retrieved.EnableFuzzyMatching.Should().BeTrue();
    }
}
