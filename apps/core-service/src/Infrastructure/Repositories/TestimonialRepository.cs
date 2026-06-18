using DeskMatch.CoreService.Application.Testimonials.Interfaces;
using DeskMatch.CoreService.Domain.Testimonials;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public class TestimonialRepository : ITestimonialRepository
{
    private readonly CoreDbContext _context;

    public TestimonialRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<Testimonial?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Testimonials.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Testimonial>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Testimonials.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Testimonial>> GetActiveTestimonialsAsync(CancellationToken cancellationToken = default)
        => await _context.Testimonials
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task<Testimonial> AddAsync(Testimonial entity, CancellationToken cancellationToken = default)
    {
        await _context.Testimonials.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(Testimonial entity) => _context.Testimonials.Update(entity);

    public void Delete(Testimonial entity) => _context.Testimonials.Remove(entity);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Testimonials.AnyAsync(t => t.Id == id, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
