using DeskMatch.CoreService.Application.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly CoreDbContext _context;

    public WorkspaceRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Workspaces.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Workspace>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Workspaces.ToListAsync(cancellationToken);

    public async Task<Workspace> AddAsync(Workspace entity, CancellationToken cancellationToken = default)
    {
        await _context.Workspaces.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(Workspace entity) => _context.Workspaces.Update(entity);

    public void Delete(Workspace entity) => _context.Workspaces.Remove(entity);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Workspaces.AnyAsync(c => c.Id == id, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}