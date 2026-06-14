#r "Planova.Shared/bin/Debug/net8.0/Planova.Shared.dll"
#r "Planova.Domain/bin/Debug/net8.0/Planova.Domain.dll"
#r "Planova.Application/bin/Debug/net8.0/Planova.Application.dll"
#r "Planova.Infrastructure/bin/Debug/net8.0/Planova.Infrastructure.dll"
#r "Planova.Persistence/bin/Debug/net8.0/Planova.Persistence.dll"
#r "Planova.Primavera/bin/Debug/net8.0/Planova.Primavera.dll"

using Microsoft.Extensions.DependencyInjection;
using Planova.Persistence.DbContext;
using Planova.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "Planova", "planova.db");

var options = new DbContextOptionsBuilder<PlanovaDbContext>()
    .UseSqlite($"Data Source={dbPath}")
    .Options;

using var ctx = new PlanovaDbContext(options, new NoopLogger());

// Check if any projects exist
var exists = await ctx.Set<Planova.Domain.Entities.Project>().AnyAsync();
if (exists)
{
    Console.WriteLine("EXISTS");
    return;
}

// Create a demo project
var project = new Planova.Domain.Entities.Project
{
    Name = "Demo Project",
    Code = "DEMO-001",
    Status = "Active",
    StartDate = DateTime.UtcNow,
    FinishDate = DateTime.UtcNow.AddMonths(6),
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};
ctx.Set<Planova.Domain.Entities.Project>().Add(project);
await ctx.SaveChangesAsync();
Console.WriteLine("SEEDED");

public class NoopLogger : ILoggingService
{
    public void Info(string message, params object[] args) {}
    public void Warn(string message, params object[] args) {}
    public void Error(string message, params object[] args) {}
    public void Error(string message, Exception ex, params object[] args) {}
    public void Debug(string message, params object[] args) {}
}
