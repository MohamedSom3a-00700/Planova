using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;
using Planova.Persistence.EntityConfigurations;
using Planova.Persistence.Repositories;

namespace Persistence.Tests;

public class ProjectRepositoryTests : IDisposable
{
    private readonly PlanovaDbContext _context;
    private readonly ProjectRepository _repo;

    public ProjectRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PlanovaDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new PlanovaDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repo = new ProjectRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsProject()
    {
        var project = new Project { Code = "P001", Name = "Test", Status = "Draft" };
        var created = await _repo.AddAsync(project);

        var result = await _repo.GetByIdAsync(created.Id);
        result.Should().NotBeNull();
        result!.Code.Should().Be("P001");
    }

    [Fact]
    public async Task UniqueCodeConstraint_Throws()
    {
        await _repo.AddAsync(new Project { Code = "P001", Name = "A", Status = "Draft" });

        Func<Task> act = () => _repo.AddAsync(new Project { Code = "P001", Name = "B", Status = "Draft" });
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        await _repo.AddAsync(new Project { Code = "P1", Name = "A", Status = "Draft" });
        await _repo.AddAsync(new Project { Code = "P2", Name = "B", Status = "Active" });

        var results = await _repo.GetAllAsync();
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_FindsByName()
    {
        await _repo.AddAsync(new Project { Code = "P1", Name = "Alpha", Status = "Draft" });
        await _repo.AddAsync(new Project { Code = "P2", Name = "Beta", Status = "Draft" });

        var results = await _repo.SearchAsync("Alpha");
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByStatusAsync_FiltersCorrectly()
    {
        await _repo.AddAsync(new Project { Code = "P1", Name = "A", Status = "Draft" });
        await _repo.AddAsync(new Project { Code = "P2", Name = "B", Status = "Completed" });

        var results = await _repo.GetByStatusAsync("Draft");
        results.Should().HaveCount(1);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
