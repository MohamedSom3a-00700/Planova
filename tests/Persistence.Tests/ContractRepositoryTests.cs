using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;
using Planova.Persistence.Repositories;

namespace Persistence.Tests;

public class ContractRepositoryTests : IDisposable
{
    private readonly PlanovaDbContext _context;
    private readonly ContractRepository _repo;

    public ContractRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PlanovaDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new PlanovaDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _context.Projects.Add(new Project { Code = "P1", Name = "Project", Status = "Draft" });
        _context.Clients.Add(new Client { Code = "C1", Name = "Client" });
        _context.SaveChanges();

        _repo = new ContractRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsContract()
    {
        var contract = new Contract
        {
            Number = "CTR001", Title = "Test", ProjectId = 1, ClientId = 1
        };
        var created = await _repo.AddAsync(contract);

        var result = await _repo.GetByIdAsync(created.Id);
        result.Should().NotBeNull();
        result!.Number.Should().Be("CTR001");
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsProjectContracts()
    {
        await _repo.AddAsync(new Contract { Number = "C1", Title = "T1", ProjectId = 1, ClientId = 1 });
        await _repo.AddAsync(new Contract { Number = "C2", Title = "T2", ProjectId = 1, ClientId = 1 });

        var results = await _repo.GetByProjectAsync(1);
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByClientAsync_ReturnsClientContracts()
    {
        await _repo.AddAsync(new Contract { Number = "C1", Title = "T1", ProjectId = 1, ClientId = 1 });

        var results = await _repo.GetByClientAsync(1);
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task UniqueNumberConstraint_Throws()
    {
        await _repo.AddAsync(new Contract { Number = "UNIQUE", Title = "A", ProjectId = 1, ClientId = 1 });

        Func<Task> act = () => _repo.AddAsync(new Contract { Number = "UNIQUE", Title = "B", ProjectId = 1, ClientId = 1 });
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
