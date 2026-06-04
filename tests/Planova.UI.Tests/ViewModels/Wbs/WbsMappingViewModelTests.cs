using FluentAssertions;
using Moq;
using Planova.Boq.Application.Services;
using Planova.Boq.Domain.Interfaces;
using Planova.UI.ViewModels.Wbs;
using Planova.Wbs.Application.Dto;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.UI.Tests.ViewModels.Wbs;

public class WbsMappingViewModelTests
{
    private readonly Mock<IBoqService> _boqService = new();
    private readonly Mock<IWbsBoqMappingService> _mappingService = new();
    private readonly Mock<IBoqImportService> _importService = new();
    private readonly Mock<IBoqSession> _session = new();
    private readonly WbsMappingViewModel _sut;

    public WbsMappingViewModelTests()
    {
        _session.Setup(s => s.CurrentProjectId).Returns(Guid.NewGuid());
        _sut = new WbsMappingViewModel(
            _boqService.Object, _mappingService.Object,
            _importService.Object, _session.Object);
    }

    [Fact]
    public void InitialState_StartsAtStep1()
    {
        _sut.CurrentStep.Should().Be(1);
        _sut.IsStep1Visible.Should().BeTrue();
        _sut.IsStep2Visible.Should().BeFalse();
        _sut.IsStep3Visible.Should().BeFalse();
        _sut.IsStep4Visible.Should().BeFalse();
    }

    [Fact]
    public void InitialState_NoSelection()
    {
        _sut.IsLoading.Should().BeFalse();
        _sut.HasError.Should().BeFalse();
        _sut.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void CanGoBack_Step1_ReturnsFalse()
    {
        _sut.CurrentStep = 1;
        _sut.CanGoBack.Should().BeFalse();
    }

    [Fact]
    public void CanGoBack_Step2_ReturnsTrue()
    {
        _sut.CurrentStep = 2;
        _sut.CanGoBack.Should().BeTrue();
    }

    [Fact]
    public void StepTransition_UpdatesVisibility()
    {
        _sut.CurrentStep = 3;
        _sut.IsStep1Visible.Should().BeFalse();
        _sut.IsStep2Visible.Should().BeFalse();
        _sut.IsStep3Visible.Should().BeTrue();
        _sut.IsStep4Visible.Should().BeFalse();
    }
}
