using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;
using Planova.Persistence.Extensions;
using Planova.Primavera.Application.Parsers;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Enums;
using Planova.Primavera.Extensions;

var cmdXerPath = "";
var cmdCommit = false;
var cmdProjectId = -1;
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--xer" && i + 1 < args.Length) cmdXerPath = args[++i];
    if (args[i] == "--commit") cmdCommit = true;
    if (args[i] == "--project" && i + 1 < args.Length) int.TryParse(args[++i], out cmdProjectId);
}

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "Planova", "planova.db");

Console.WriteLine($"Database: {dbPath}");
if (!File.Exists(dbPath))
{
    Console.WriteLine("No Planova database found. Starting fresh.");
}

var services = new ServiceCollection();
services.AddDbContext<PlanovaDbContext>(o =>
    o.UseSqlite($"Data Source={dbPath}"));
services.AddPlanovaPersistence();
services.AddPlanovaPrimavera();
services.AddScoped<XerParser>();

var sp = services.BuildServiceProvider();

// --- Step 1: Choose Project ---
using var scope = sp.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<PlanovaDbContext>();
await ctx.Database.EnsureCreatedAsync();

var projects = await ctx.Set<Project>().OrderBy(p => p.Name).ToListAsync();

Project? selectedProject = null;
if (cmdProjectId >= 0)
{
    selectedProject = projects.FirstOrDefault(p => p.Id == cmdProjectId);
    if (selectedProject == null)
        Console.WriteLine($"Project ID {cmdProjectId} not found. Showing selection.");
}

if (selectedProject == null)
{
    if (projects.Count == 0)
    {
        Console.WriteLine("No projects found. Creating a new one.");
        selectedProject = new Project
        {
            Code = "IMP-001",
            Name = "Imported Project",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.Set<Project>().Add(selectedProject);
        await ctx.SaveChangesAsync();
        Console.WriteLine($"Created project: {selectedProject.Name} (ID: {selectedProject.Id})");
    }
    else
    {
        Console.WriteLine("\nAvailable projects:");
        for (int i = 0; i < projects.Count; i++)
            Console.WriteLine($"  [{i + 1}] {projects[i].Name} (ID: {projects[i].Id})");
        Console.WriteLine($"  [{projects.Count + 1}] Create new project");

        Console.Write("\nSelect project: ");
        var input = Console.ReadLine();
        if (int.TryParse(input, out int idx) && idx >= 1 && idx <= projects.Count)
        {
            selectedProject = projects[idx - 1];
        }
        else
        {
            Console.Write("Enter new project name: ");
            var name = Console.ReadLine() ?? "Imported Project";
            selectedProject = new Project
            {
                Code = $"IMP-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Name = name,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            ctx.Set<Project>().Add(selectedProject);
            await ctx.SaveChangesAsync();
        }
    }
}
Console.WriteLine($"Selected project: {selectedProject.Name}");

// --- Step 2: Parse XER ---
var xerPath = cmdXerPath;
if (string.IsNullOrEmpty(xerPath))
{
    Console.Write("\nEnter XER file path: ");
    xerPath = Console.ReadLine();
}
while (string.IsNullOrEmpty(xerPath) || !File.Exists(xerPath))
{
    Console.Write("File not found. Enter valid XER path: ");
    xerPath = Console.ReadLine();
}

Console.WriteLine("\nParsing XER file...");
var parser = scope.ServiceProvider.GetRequiredService<XerParser>();
var result = await parser.ParseAsync(xerPath);

Console.WriteLine();
if (result.Errors.Count > 0)
{
    Console.WriteLine($"Errors ({result.Errors.Count}):");
    foreach (var err in result.Errors)
        Console.WriteLine($"  ERROR: {err}");
}

if (result.Warnings.Count > 0)
{
    Console.WriteLine($"Warnings ({result.Warnings.Count}):");
    foreach (var warn in result.Warnings)
        Console.WriteLine($"  WARN: {warn}");
}

Console.WriteLine("\n--- Parse Results ---");
Console.WriteLine($"Project:      {result.Project?.Name ?? "(none)"}");
Console.WriteLine($"Calendars:    {result.Calendars.Count}");
Console.WriteLine($"Activities:   {result.Activities.Count}");
Console.WriteLine($"Relationships:{result.Relationships.Count}");
Console.WriteLine($"Assignments:  {result.ResourceAssignments.Count}");
Console.WriteLine($"Codes:        {result.Codes.Count}");
Console.WriteLine($"UDFs:         {result.Udfs.Count}");
Console.WriteLine($"Baselines:    {result.Baselines.Count}");
Console.WriteLine($"Raw tables:   {result.RawTables.Count}");

if (result.RowCounts.Count > 0)
{
    Console.WriteLine("\nRow counts:");
    foreach (var kv in result.RowCounts.OrderBy(r => r.Key))
        Console.WriteLine($"  {kv.Key}: {kv.Value}");
}

var commit = cmdCommit ? "y" : null;
if (!cmdCommit)
{
    Console.Write("\nCommit to database? (y/N): ");
    commit = Console.ReadLine();
}
if (string.Equals(commit, "y", StringComparison.OrdinalIgnoreCase))
{
    var session = new XerImportSession
    {
        Id = Guid.NewGuid(),
        Status = PrimaveraImportStatus.Committed,
        SourceFileName = Path.GetFileName(xerPath),
        SourceFileHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(xerPath))
        ).ToLowerInvariant(),
        ImportedAt = DateTime.UtcNow,
        ImportedBy = Environment.UserName,
        RowCounts = JsonSerializer.Serialize(result.RowCounts)
    };
    ctx.Set<XerImportSession>().Add(session);

    foreach (var cal in result.Calendars)
        ctx.Set<PrimaveraCalendar>().Add(cal);
    foreach (var act in result.Activities)
        ctx.Set<PrimaveraActivity>().Add(act);
    foreach (var rel in result.Relationships)
        ctx.Set<PrimaveraRelationship>().Add(rel);
    foreach (var ra in result.ResourceAssignments)
        ctx.Set<PrimaveraResourceAssignment>().Add(ra);
    foreach (var code in result.Codes)
        ctx.Set<PrimaveraCode>().Add(code);
    foreach (var udf in result.Udfs)
        ctx.Set<PrimaveraUdf>().Add(udf);
    foreach (var bl in result.Baselines)
        ctx.Set<PrimaveraBaseline>().Add(bl);
    foreach (var rt in result.RawTables)
        ctx.Set<XerRawTable>().Add(rt);

    await ctx.SaveChangesAsync();
    Console.WriteLine($"Committed {result.Calendars.Count + result.Activities.Count + result.Relationships.Count + result.ResourceAssignments.Count + result.Codes.Count + result.Udfs.Count + result.Baselines.Count + result.RawTables.Count} entities.");
}
else
{
    Console.WriteLine("Skipped commit.");
}

Console.WriteLine("\nDone.");
