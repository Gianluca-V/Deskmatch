using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public class WorkspaceBlockRepository : IWorkspaceBlockRepository
{
    private readonly CoreDbContext _context;

    public WorkspaceBlockRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkspaceBlock>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken)
        => await _context.WorkspaceBlocks
            .Where(b => b.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);

    public async Task<WorkspaceBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.WorkspaceBlocks
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<Guid> AddAsync(WorkspaceBlock block, CancellationToken cancellationToken)
    {
        _context.WorkspaceBlocks.Add(block);
        await _context.SaveChangesAsync(cancellationToken);
        return block.Id;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var block = await GetByIdAsync(id, cancellationToken);
        if (block is not null)
        {
            _context.WorkspaceBlocks.Remove(block);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}