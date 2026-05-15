using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MathTestSystem.Infrastructure.Data;

/// <summary>
/// Used by EF Core Tools at design time to instantiate AppDbContext
/// when running migrations (Add-Migration, Update-Database, etc.).
/// Not used at runtime — the application registers its own DbContext via DI.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();

        // Design-time only connection string.
        // The runtime connection string is provided via appsettings.json in each API service.
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=MathTestDb;User Id=sa;Password=MathTest_Sa!2024;TrustServerCertificate=True;");

        return new AppDbContext(optionsBuilder.Options);
    }
}
