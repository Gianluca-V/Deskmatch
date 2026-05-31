using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DeskMatch.CoreService.Infrastructure.Persistence;

public class CoreDbContextFactory : IDesignTimeDbContextFactory<CoreDbContext>
{
    public CoreDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoreDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=deskmatch_core;Username=deskmatch;Password=deskmatch123!");

        return new CoreDbContext(optionsBuilder.Options);
    }
}