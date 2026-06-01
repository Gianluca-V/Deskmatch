using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DeskMatch.AuthService.Infrastructure.Persistence;

// Usado solo por dotnet-ef en tiempo de diseño (migrations add / database update).
// Nunca se instancia en runtime.
public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=deskmatch_auth;Username=deskmatch;Password=deskmatch123!")
            .Options;

        return new AuthDbContext(options);
    }
}
