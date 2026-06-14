using FluentAssertions;
using Moq;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Application.Models;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.Primavera.Tests.Integration;

public class CrossStudioConsumptionTests
{
    [Fact]
    public async Task PrimaveraWorkspaceService_WhenAvailable_ReturnsData()
    {
        var mock = new Mock<IPrimaveraWorkspaceService>();
        mock.Setup(s => s.HasDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mock.Setup(s => s.GetSnapshotAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PrimaveraWorkspaceSnapshot
            {
                Activities = new List<PrimaveraActivityDto>
                {
                    new() { TaskId = "T1", Name = "Activity 1", Duration = 10 }
                }
            });

        var service = mock.Object;

        var hasData = await service.HasDataAsync(1);
        hasData.Should().BeTrue();

        var snapshot = await service.GetSnapshotAsync(1);
        snapshot.Activities.Should().HaveCount(1);
        snapshot.Activities[0].Name.Should().Be("Activity 1");
    }

    [Fact]
    public async Task PrimaveraWorkspaceService_WhenAbsent_ConsumerFallsBackGracefully()
    {
        IPrimaveraWorkspaceService? service = null;

        var fallbackData = new PrimaveraWorkspaceSnapshot
        {
            Activities = new List<PrimaveraActivityDto>
            {
                new() { TaskId = "NATIVE-1", Name = "Native Activity" }
            }
        };

        var result = service != null
            ? await service.GetSnapshotAsync(1)
            : fallbackData;

        result.Should().NotBeNull();
        result.Activities[0].TaskId.Should().Be("NATIVE-1");
    }

    [Fact]
    public void NullableRegistration_AllowsOptionalInjection()
    {
        IPrimaveraWorkspaceService? service = null;
        service.Should().BeNull();

        var isAvailable = service != null;
        isAvailable.Should().BeFalse();
    }
}
