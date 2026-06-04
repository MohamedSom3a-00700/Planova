using FluentAssertions;
using Moq;
using Planova.Boq.Application.Services;
using Planova.Boq.Domain.Interfaces;
using Planova.UI.ViewModels.Wbs;
using Planova.Wbs.Application.Dto;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Enums;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.UI.Tests.ViewModels.Wbs;

public class WbsAiGenerationViewModelTests
{
    private readonly Mock<IWbsAiGenerationService> _aiService = new();
    private readonly Mock<IWbsService> _wbsService = new();
    private readonly Mock<IBoqService> _boqService = new();
    private readonly Mock<IBoqImportService> _importService = new();
    private readonly Mock<IBoqSession> _session = new();
    private readonly Mock<IWbsItemRepository> _itemRepo = new();
    private readonly WbsAiGenerationViewModel _sut;

    public WbsAiGenerationViewModelTests()
    {
        _session.Setup(s => s.CurrentProjectId).Returns(Guid.NewGuid());
        _sut = new WbsAiGenerationViewModel(
            _aiService.Object, _wbsService.Object, _boqService.Object,
            _importService.Object, _itemRepo.Object, _session.Object);
    }

    [Fact]
    public void InitialState_IsEmpty()
    {
        _sut.IsLoading.Should().BeFalse();
        _sut.HasError.Should().BeFalse();
        _sut.ErrorMessage.Should().BeEmpty();
        _sut.ProjectScope.Should().BeEmpty();
        _sut.SuggestedTree.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckAvailabilityAsync_SetsIsAiAvailable()
    {
        _aiService.Setup(s => s.IsAiAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.CheckAvailabilityCommand.ExecuteAsync(null);

        _sut.IsAiAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_EmptyScope_DoesNothing()
    {
        _sut.ProjectScope = "";

        await _sut.GenerateCommand.ExecuteAsync(null);

        _aiService.Verify(s => s.GenerateAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        _sut.SuggestedTree.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateAsync_ValidScope_PopulatesTree()
    {
        _sut.ProjectScope = "Build a bridge";
        var items = new List<SuggestedItem>
        {
            new(Guid.NewGuid(), null, "Design", null, 0, 0, "Summary", null, new List<SuggestedItem>())
        };
        _aiService.Setup(s => s.GenerateAsync("Build a bridge", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WbsGenerationResult(items, true));

        await _sut.GenerateCommand.ExecuteAsync(null);

        _sut.SuggestedTree.Should().NotBeEmpty();
        _sut.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateAsync_AiNotAvailable_SetsError()
    {
        _sut.ProjectScope = "Build a bridge";
        _aiService.Setup(s => s.GenerateAsync("Build a bridge", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WbsGenerationResult(new List<SuggestedItem>(), false));

        await _sut.GenerateCommand.ExecuteAsync(null);

        _sut.HasError.Should().BeTrue();
        _sut.ErrorMessage.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AcceptAsync_NoTree_DoesNothing()
    {
        await _sut.AcceptCommand.ExecuteAsync(null);

        _wbsService.Verify(s => s.CreateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<WbsSource>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AcceptAsync_WithTree_CreatesWbs()
    {
        var projectHash = _session.Object.CurrentProjectId!.Value.GetHashCode();
        _sut.SuggestedTree.Add(new SuggestedItemNode
        {
            Id = Guid.NewGuid(),
            Name = "Design",
            Level = 0,
            WbsLevel = "Summary"
        });
        _wbsService.Setup(s => s.CreateAsync(It.IsAny<string>(), projectHash, WbsSource.AIGenerated, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Planova.Wbs.Domain.Entities.Wbs { Id = Guid.NewGuid(), Name = "AI Generated WBS" });

        await _sut.AcceptCommand.ExecuteAsync(null);

        _wbsService.Verify(s => s.CreateAsync(It.IsAny<string>(), projectHash, WbsSource.AIGenerated, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegenerateAsync_CallsGenerate()
    {
        _sut.ProjectScope = "Build a bridge";
        _aiService.Setup(s => s.GenerateAsync("Build a bridge", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WbsGenerationResult(new List<SuggestedItem>(), true));

        await _sut.RegenerateCommand.ExecuteAsync(null);

        _aiService.Verify(s => s.GenerateAsync("Build a bridge", null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
