using FluentAssertions;
using Planova.Domain.Entities;

namespace Domain.Tests;

public class ProjectTests
{
    [Fact]
    public void NewProject_HasDefaultValues()
    {
        var project = new Project();
        project.Status.Should().BeEmpty();
        project.Code.Should().BeEmpty();
        project.Contracts.Should().BeEmpty();
    }

    [Fact]
    public void CanSetProperties()
    {
        var project = new Project
        {
            Code = "PRJ001",
            Name = "Test Project",
            Status = "Draft"
        };

        project.Code.Should().Be("PRJ001");
        project.Name.Should().Be("Test Project");
        project.Status.Should().Be("Draft");
    }
}
