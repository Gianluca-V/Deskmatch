using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.Domain.Abstractions;

namespace DeskMatch.CoreService.Application.Workspaces.Interfaces;

public interface IWorkspaceRepository : IRepository<Workspace, Guid>
{
    Task<IReadOnlyList<Workspace>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Workspace> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? city,
        string? country,
        decimal? minPrice,
        decimal? maxPrice,
        int? minCapacity,
        string? amenities,
        CancellationToken cancellationToken = default);
}