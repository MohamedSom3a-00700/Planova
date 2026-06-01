using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;
using Planova.Persistence.Repositories;

namespace Persistence.Tests;

public class ClientRepositoryTests : IDisposable
{
    private readonly PlanovaDbContext _context;
    private readonly ClientRepository _repo;

    public ClientRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PlanovaDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new PlanovaDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repo = new ClientRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsClient()
    {
        var client = new Client { Code = "C001", Name = "Test Client" };
        var created = await _repo.AddAsync(client);

        var result = await _repo.GetByIdAsync(created.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Client");
    }

    [Fact]
    public async Task UniqueCodeConstraint_Throws()
    {
        await _repo.AddAsync(new Client { Code = "C001", Name = "A" });

        Func<Task> act = () => _repo.AddAsync(new Client { Code = "C001", Name = "B" });
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task UniqueNameConstraint_Throws()
    {
        await _repo.AddAsync(new Client { Code = "C001", Name = "Same" });

        Func<Task> act = () => _repo.AddAsync(new Client { Code = "C002", Name = "Same" });
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task HasLinkedProjectsAsync_ReturnsTrue_WhenLinked()
    {
        var client = await _repo.AddAsync(new Client { Code = "C001", Name = "Client" });
        _context.Projects.Add(new Project { Code = "P1", Name = "Proj", Status = "Draft", ClientId = client.Id });
        await _context.SaveChangesAsync();

        var hasLinks = await _repo.HasLinkedProjectsAsync(client.Id);
        hasLinks.Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
