using System.ComponentModel.DataAnnotations;

namespace DeskMatch.CoreService.Application.Workspaces.Dtos;

public record UpdateWorkspaceScheduleRequest(
    DayOfWeek DayOfWeek,

    [Required]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "OpenTime debe tener formato HH:mm")]
    string OpenTime,

    [Required]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "CloseTime debe tener formato HH:mm")]
    string CloseTime,

    bool IsAvailable
);