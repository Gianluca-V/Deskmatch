namespace DeskMatch.CoreService.Application.Dashboard.Dtos;

public sealed record WorkspaceRevenueDto(
    string WorkspaceName,
    decimal TotalRevenue
);
