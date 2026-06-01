using FluentAssertions;
using Planova.Domain.Entities;

namespace Domain.Tests;

public class ContractTests
{
    [Fact]
    public void NewContract_HasRequiredRefs()
    {
        var contract = new Contract();
        contract.Number.Should().BeEmpty();
        contract.Title.Should().BeEmpty();
    }

    [Fact]
    public void CanSetProperties()
    {
        var contract = new Contract
        {
            Number = "CTR001",
            Title = "Test Contract",
            Value = 100000m,
            ProjectId = 1,
            ClientId = 2
        };

        contract.Number.Should().Be("CTR001");
        contract.Title.Should().Be("Test Contract");
        contract.Value.Should().Be(100000m);
    }
}
