using DeskMatch.CoreService.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.CoreService.Domain.Audit;

namespace DeskMatch.CoreService.Infrastructure.Persistence;

public class CoreDbContext : DbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options)
        : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}