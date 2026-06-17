using DeskMatch.CoreService.Domain.Companies;
using DeskMatch.Domain.Abstractions;

namespace DeskMatch.CoreService.Application.Companies.Interfaces;

public interface ICompanyRepository : IRepository<Company, Guid>
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Company?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
}