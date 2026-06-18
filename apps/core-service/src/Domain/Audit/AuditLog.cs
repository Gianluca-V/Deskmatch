using DeskMatch.Domain.Base;

namespace DeskMatch.CoreService.Domain.Audit;

public class AuditLog : Entity<Guid>
{
    public AuditLog(Guid id, Guid adminId, string action, string targetType, Guid targetId, string? reason)
    {
        Id = id;
        AdminId = adminId;
        Action = action;
        TargetType = targetType;
        TargetId = targetId;
        Reason = reason;
        CreatedAt = DateTime.UtcNow;
    }

    private AuditLog() { } 

    public Guid AdminId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string TargetType { get; private set; } = string.Empty;
    public Guid TargetId { get; private set; }
    public string? Reason { get; private set; }
}