using DeskMatch.CoreService.Domain.Companies;
using DeskMatch.CoreService.Domain.Payments;
using DeskMatch.CoreService.Domain.Reservations;
using DeskMatch.CoreService.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Persistence;

public class CoreDbContext : DbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options)
        : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}