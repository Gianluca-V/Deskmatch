using DeskMatch.CoreService.Application.Workspaces.Models;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Commands;

public sealed record BulkConfirmCommand(
    Guid CompanyId,
    IReadOnlyList<OfficeData> Offices,
    IReadOnlyList<BulkImageFile> Images
) : ICommand<BulkCreateResponse>;

public sealed record BulkImageFile(string FileName, byte[] Content);
