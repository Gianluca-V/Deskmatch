using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly CoreDbContext _context;

    public WorkspaceRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Workspaces.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Workspace>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Workspaces.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Workspace>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
        => await _context.Workspaces.Where(w => w.CompanyId == companyId && w.IsActive).ToListAsync(cancellationToken);

    public async Task<Workspace> AddAsync(Workspace entity, CancellationToken cancellationToken = default)
    {
        await _context.Workspaces.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(Workspace entity) => _context.Workspaces.Update(entity);

    public void Delete(Workspace entity) => _context.Workspaces.Remove(entity);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Workspaces.AnyAsync(c => c.Id == id, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task<(IReadOnlyList<Workspace> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? city,
        string? country,
        decimal? minPrice,
        decimal? maxPrice,
        int? minCapacity,
        string? amenities,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Workspaces.AsQueryable();

        // Filtrar solo activos
        query = query.Where(w => w.IsActive);

        // Filtros opcionales
        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(w => w.City != null && w.City == city);

        if (!string.IsNullOrWhiteSpace(country))
            query = query.Where(w => w.Country != null && w.Country == country);

        if (minPrice.HasValue)
            query = query.Where(w => w.PricePerHour >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(w => w.PricePerHour <= maxPrice.Value);

        if (minCapacity.HasValue)
            query = query.Where(w => w.Capacity >= minCapacity.Value);

        if (!string.IsNullOrWhiteSpace(amenities))
        {
            var amenityList = amenities.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            query = query.Where(w => w.Amenities != null && amenityList.All(a => w.Amenities.Contains(a)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(w => w.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}