using FluentAssertions;
using Planova.Domain.Entities;

namespace Domain.Tests;

public class ClientTests
{
    [Fact]
    public void NewClient_HasEmptyCollections()
    {
        var client = new Client();
        client.Projects.Should().BeEmpty();
        client.Contracts.Should().BeEmpty();
    }

    [Fact]
    public void CanSetProperties()
    {
        var client = new Client
        {
            Code = "CLT001",
            Name = "Test Client",
            ContactEmail = "test@example.com"
        };

        client.Code.Should().Be("CLT001");
        client.Name.Should().Be("Test Client");
        client.ContactEmail.Should().Be("test@example.com");
    }
}
