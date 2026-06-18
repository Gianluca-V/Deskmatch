using DeskMatch.CoreService.Domain.Companies;
using DeskMatch.CoreService.Domain.Testimonials;
using DeskMatch.CoreService.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Persistence;

public class CoreDbContext : DbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options)
        : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}