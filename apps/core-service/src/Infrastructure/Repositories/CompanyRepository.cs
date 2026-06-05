using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Domain.Companies;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly CoreDbContext _context;

    public CompanyRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Company?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.Companies.FirstOrDefaultAsync(c => c.OwnerId == ownerId, cancellationToken);

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Companies.ToListAsync(cancellationToken);

    public async Task<Company> AddAsync(Company entity, CancellationToken cancellationToken = default)
    {
        await _context.Companies.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(Company entity) => _context.Companies.Update(entity);

    public void Delete(Company entity) => _context.Companies.Remove(entity);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Companies.AnyAsync(c => c.Id == id, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}