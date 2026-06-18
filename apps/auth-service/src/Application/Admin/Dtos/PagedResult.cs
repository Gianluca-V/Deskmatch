namespace DeskMatch.AuthService.Application.Admin.Dtos;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Skip,
    int Take
);