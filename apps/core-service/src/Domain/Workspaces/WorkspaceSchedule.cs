using DeskMatch.Domain.Base;

namespace DeskMatch.CoreService.Domain.Workspaces;

public class WorkspaceSchedule : AggregateRoot<Guid>
{
    public WorkspaceSchedule(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    private WorkspaceSchedule() { } // EF Core

    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public bool IsAvailable { get; set; } = true;
}