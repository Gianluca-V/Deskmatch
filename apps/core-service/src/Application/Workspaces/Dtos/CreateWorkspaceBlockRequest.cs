using System.ComponentModel.DataAnnotations;

namespace DeskMatch.CoreService.Application.Workspaces.Dtos;

public record CreateWorkspaceBlockRequest(
    [Required]
    DateTimeOffset BlockStart,

    [Required]
    DateTimeOffset BlockEnd,

    string? Reason
);