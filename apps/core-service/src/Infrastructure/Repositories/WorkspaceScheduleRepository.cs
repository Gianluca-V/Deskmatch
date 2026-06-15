using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public class WorkspaceScheduleRepository : IWorkspaceScheduleRepository
{
    private readonly CoreDbContext _context;

    public WorkspaceScheduleRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkspaceSchedule>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken)
        => await _context.WorkspaceSchedules
            .Where(s => s.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);

    public async Task<WorkspaceSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.WorkspaceSchedules
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<Guid> AddAsync(WorkspaceSchedule schedule, CancellationToken cancellationToken)
    {
        _context.WorkspaceSchedules.Add(schedule);
        await _context.SaveChangesAsync(cancellationToken);
        return schedule.Id;
    }

    public async Task UpdateAsync(WorkspaceSchedule schedule, CancellationToken cancellationToken)
    {
        _context.WorkspaceSchedules.Update(schedule);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await GetByIdAsync(id, cancellationToken);
        if (schedule is not null)
        {
            _context.WorkspaceSchedules.Remove(schedule);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}