using FluentAssertions;
using Planova.Activity.Domain.Entities;

namespace Planova.Activity.Tests.Domain;

public class CalendarTests
{
    [Fact]
    public void Calendar_WithWorkingDays_ShouldSucceed()
    {
        var calendar = new Calendar
        {
            Id = Guid.NewGuid(),
            Name = "Standard 5-Day",
            IsDefault = true,
            Monday = true,
            Tuesday = true,
            Wednesday = true,
            Thursday = true,
            Friday = true,
            Saturday = false,
            Sunday = false
        };

        calendar.Monday.Should().BeTrue();
        calendar.Saturday.Should().BeFalse();
        calendar.Sunday.Should().BeFalse();
        calendar.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void Calendar_GetDefault_ShouldReturnStandard5Day()
    {
        var calendar = Calendar.GetDefault();

        calendar.Should().NotBeNull();
        calendar.Name.Should().Be("Standard 5-Day Work Week");
        calendar.Monday.Should().BeTrue();
        calendar.Sunday.Should().BeTrue();
        calendar.Saturday.Should().BeFalse();
        calendar.Friday.Should().BeFalse();
    }
}
