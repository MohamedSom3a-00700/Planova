using FluentAssertions;
using Moq;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Application.Services;
using Planova.Primavera.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Primavera.Tests.Application;

public class PrimaveraRepairServiceTests
{
    private readonly Mock<IPrimaveraValidationService> _validationMock = new();
    private readonly Mock<IPrimaveraRepairRepository> _repoMock = new();
    private readonly Mock<IPrimaveraWorkspaceService> _workspaceMock = new();
    private readonly Mock<ILoggingService> _logMock = new();
    private readonly PrimaveraRepairService _service;

    public PrimaveraRepairServiceTests()
    {
        _service = new PrimaveraRepairService(
            _validationMock.Object, _repoMock.Object,
            _workspaceMock.Object, _logMock.Object);
    }

    [Fact]
    public async Task GetSuggestedFixesAsync_WithNoIssues_ReturnsEmpty()
    {
        _validationMock.Setup(v => v.ValidateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PrimaveraValidationIssueDto>());

        var result = await _service.GetSuggestedFixesAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSuggestedFixesAsync_WithIssues_ReturnsFixes()
    {
        var issues = new List<PrimaveraValidationIssueDto>
        {
            new() { Severity = "Error", EntityType = "Activity", Description = "Missing calendar", SuggestedFix = "Assign calendar" }
        };

        _validationMock.Setup(v => v.ValidateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(issues);
        _repoMock.Setup(r => r.AddActionAsync(It.IsAny<Primavera.Domain.Entities.PrimaveraRepairAction>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Primavera.Domain.Entities.PrimaveraRepairAction a, CancellationToken _) => a);

        var result = await _service.GetSuggestedFixesAsync(1);

        result.Should().NotBeEmpty();
        result.Should().Contain(f => f.Description == "Assign calendar");
    }
}
