using DeskMatch.CoreService.Domain.Companies;
using DeskMatch.Domain.Abstractions;

namespace DeskMatch.CoreService.Application.Companies.Interfaces;

public interface ICompanyRepository : IRepository<Company, Guid>
{
    Task<Company?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Company> Items, int Total)> GetPagedAsync(int skip, int take, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}