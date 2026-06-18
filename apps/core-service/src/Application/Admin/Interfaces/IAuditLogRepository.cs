using DeskMatch.CoreService.Domain.Audit;

namespace DeskMatch.CoreService.Application.Admin.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetAllAsync(int skip, int take, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}