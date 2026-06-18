using DeskMatch.CoreService.Application.Admin.Interfaces;
using DeskMatch.CoreService.Domain.Audit;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly CoreDbContext _context;

    public AuditLogRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
        => await _context.AuditLogs.AddAsync(log, ct);

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync(int skip, int take, CancellationToken ct = default)
        => await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}