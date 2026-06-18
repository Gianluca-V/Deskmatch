namespace DeskMatch.CoreService.Application.Workspaces.Dtos;

public record WorkspaceBlockResponse(
    Guid Id,
    Guid WorkspaceId,
    DateTimeOffset BlockStart,
    DateTimeOffset BlockEnd,
    string? Reason
);