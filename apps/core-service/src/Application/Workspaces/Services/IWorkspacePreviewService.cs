using DeskMatch.CoreService.Application.Workspaces.Models;

namespace DeskMatch.CoreService.Application.Workspaces.Services;

public interface IWorkspacePreviewService
{
    Task<BulkPreviewResponse> PreviewAsync(Stream excelStream, CancellationToken ct = default);
}
