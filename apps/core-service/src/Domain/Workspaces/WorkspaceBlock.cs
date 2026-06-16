using DeskMatch.Domain.Base;

namespace DeskMatch.CoreService.Domain.Workspaces;

public class WorkspaceBlock : AggregateRoot<Guid>
{
    public WorkspaceBlock(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    private WorkspaceBlock() { } // EF Core

    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public DateTime BlockStart { get; set; }
    public DateTime BlockEnd { get; set; }
    public string? Reason { get; set; }
}