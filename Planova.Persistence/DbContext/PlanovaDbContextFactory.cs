using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Planova.Persistence.DbContext;

public class PlanovaDbContextFactory : IDesignTimeDbContextFactory<PlanovaDbContext>
{
    public PlanovaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlanovaDbContext>();
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "Planova");
        Directory.CreateDirectory(dir);
        optionsBuilder.UseSqlite($"Data Source={Path.Combine(dir, "planova.db")}");

        return new PlanovaDbContext(optionsBuilder.Options);
    }
}
